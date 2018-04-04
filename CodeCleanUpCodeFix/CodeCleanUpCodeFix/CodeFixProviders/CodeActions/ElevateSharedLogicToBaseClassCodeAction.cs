using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeCleanUpCodeFix.Helpers.SyntaxExtensions;
using CodeCleanUpCodeFix.Helpers.SyntaxHelpers;
using CodeCleanUpCodeFix.Helpers.WinApiMessage;
using CodeCleanUpCodeFix.Helpers.WinApiMessage.Interfaces;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeCleanUpCodeFix.CodeFixProviders.CodeActions
{
    class ElevateSharedLogicToBaseClassCodeAction : CodeAction
    {
        private Dictionary<ClassDeclarationSyntax, ClassUpdateDefinition> _originalToModifiedClassesDictionary;

        private Solution _solution;

        private Compilation _compilation;

        private List<ClassDeclarationSyntax> _originalAllClasses;

        private readonly Func<CancellationToken, Task<Solution>> _createChangedSolution;
        private IWinApiMessageBox _winApiMessageBox;

        public IWinApiMessageBox MessageBox
        {
            get
            {
                if (_winApiMessageBox == null)
                {
                    _winApiMessageBox = new WinApiMessageBox();
                }

                return _winApiMessageBox;
            }
            set
            {
                _winApiMessageBox = value;
            }
        }

        public override string Title { get; }

        public override string EquivalenceKey { get; }

        public ElevateSharedLogicToBaseClassCodeAction(
            Document document,
            List<ClassDeclarationSyntax> classesContainingSharedLogic,
            CancellationToken cancellationToken)
        {
            _createChangedSolution = c => ElevateSharedLogicToBaseClass(
                document,
                classesContainingSharedLogic,
                cancellationToken);

            Title = "Elevate shared logic to base class";
            EquivalenceKey = "Elevate shared logic to base class";

            _originalToModifiedClassesDictionary = new Dictionary<ClassDeclarationSyntax, ClassUpdateDefinition>();

            foreach (var classToModify in classesContainingSharedLogic)
            {
                var classUpdateDefinition = new ClassUpdateDefinition()
                {
                    NewClassDeclarationSyntax = classToModify
                };

                _originalToModifiedClassesDictionary.Add(classToModify, classUpdateDefinition);
            }
        }

        protected override Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Enumerable.Empty<CodeActionOperation>());
        }

        protected override Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
        {
            return _createChangedSolution(cancellationToken);
        }

        private Task<Solution> ElevateSharedLogicToBaseClass(
            Document document,
            List<ClassDeclarationSyntax> classesContainingSharedLogic,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromResult(document.Project.Solution);
            }

            _solution = document.Project.Solution;
            _compilation = document.Project.GetCompilationAsync(cancellationToken).GetAwaiter().GetResult();
            _originalAllClasses = _solution.DescendantNodes<ClassDeclarationSyntax>();


            if (!ClassDeclarationSyntaxHelper.CheckIfAllClassesHaveSameBaseClass(classesContainingSharedLogic))
            {
                TryUnifyBaseClass(classesContainingSharedLogic);
            }

            var modifiedClasses = _originalToModifiedClassesDictionary.Values
                .Select(classUpdateDefinition => classUpdateDefinition.NewClassDeclarationSyntax).ToList();

            if (!ClassDeclarationSyntaxHelper.CheckIfAllClassesHaveSameBaseClass(modifiedClasses))
            {
                MessageBox.Show("Refactoring Failed", "Can't unify base classes - it needs to be done manually before elevating shared logic.");
                return Task.FromResult(_solution);
            }

            var baseClass = GetBaseClass(modifiedClasses.First());

            //if (!CheckIfSharedLogicCanBeAddedToBaseClass(baseClass, classesContainingSharedLogic))
            //{
            //    baseClass = CreateIntermediateBaseClassForSharedLogic(baseClass, classesContainingSharedLogic);
            //}

            _originalToModifiedClassesDictionary.Add(baseClass, new ClassUpdateDefinition()
            {
                NewClassDeclarationSyntax = baseClass,
                NewClassUsings = new SyntaxList<UsingDirectiveSyntax>()
            });

            ElevateSharedLogicToBaseClass(classesContainingSharedLogic, baseClass, cancellationToken);


            //AddPropertyToBaseClass(ref solution, baseClass, propertiesToElevate.First());




            UpdateSolutionWithModifiedClasses();

            return Task.FromResult(_solution);
        }

        private void UpdateSolutionWithModifiedClasses()
        {
            foreach (var originalToModifiedClass in _originalToModifiedClassesDictionary)
            {
                
                var document = _solution.GetDocumentContainingNode(originalToModifiedClass.Key);
                if (document == null)
                {
                    continue;
                }

                var documentRoot = document.GetSyntaxRootAsync().GetAwaiter().GetResult();
                var root = documentRoot.ReplaceNode(originalToModifiedClass.Key, originalToModifiedClass.Value.NewClassDeclarationSyntax);

                if (originalToModifiedClass.Value.NewClassUsings.Count > 0)
                {
                    var compilationUnit = root as CompilationUnitSyntax;
                    if (compilationUnit != null)
                    {
                        root = compilationUnit.WithUsings(originalToModifiedClass.Value.NewClassUsings);
                    }
                }
                _solution = _solution.WithDocumentSyntaxRoot(document.Id, root);
                
                
            }
        }

        private ClassDeclarationSyntax GetBaseClass(ClassDeclarationSyntax childClass)
        {
            var baseClassName = childClass.GetBaseClassName();

            if (baseClassName != string.Empty)
            {
                var baseClassDeclarationSyntax = _originalAllClasses.FirstOrDefault(baseClass => baseClass.Identifier.ValueText == baseClassName);
                return baseClassDeclarationSyntax;
            }

            return null;
        }

        private void TryUnifyBaseClass(List<ClassDeclarationSyntax> classesContainingSharedLogic)
        {
            var longestInheritanceChain = new List<string>();
            var longerInheritanceChain = new List<string>();
            var shorterInheritanceChain = new List<string>();
            ClassDeclarationSyntax classWithTheLongestInheritanceChain = null;
            SyntaxList<UsingDirectiveSyntax> classWithTheLongestInheritanceChainUsings = new SyntaxList<UsingDirectiveSyntax>();

            foreach (var classToCheck in classesContainingSharedLogic)
            {
                List<string> inheritanceChain = GetInheritanceChain(classToCheck);
                if (inheritanceChain.Count > longestInheritanceChain.Count)
                {
                    longerInheritanceChain = inheritanceChain;
                    shorterInheritanceChain = longestInheritanceChain;
                    classWithTheLongestInheritanceChain = classToCheck;
                }
                else
                {
                    longerInheritanceChain = longestInheritanceChain;
                    shorterInheritanceChain = inheritanceChain;
                }

                for (var i = 0; i < shorterInheritanceChain.Count; i++)
                {
                    if (shorterInheritanceChain[i] != longestInheritanceChain[i])
                    {
                        return;
                    }
                }

                longestInheritanceChain = longerInheritanceChain;
            }

            var newCommonBaseClass = longestInheritanceChain.LastOrDefault();

            if (classWithTheLongestInheritanceChain != null)
            {
                var compilationUnit = classWithTheLongestInheritanceChain.Ancestors().OfType<CompilationUnitSyntax>().First();
                classWithTheLongestInheritanceChainUsings = compilationUnit.Usings;

            }
            
            foreach (var classToExamine in classesContainingSharedLogic)
            {
                if (classToExamine.GetBaseClassName() != newCommonBaseClass)
                {
                    var newBaseList = SyntaxFactory.BaseList();
                    var newSeparatedSyntaxList = newBaseList.Types.Add(SyntaxFactory.SimpleBaseType(
                        SyntaxFactory.IdentifierName(newCommonBaseClass)));
                    newBaseList = newBaseList.WithTypes(newSeparatedSyntaxList);

                    
                    var oldCompilationUnit = classToExamine.Ancestors().OfType<CompilationUnitSyntax>().First();
                    var oldUsings = oldCompilationUnit.Usings;
                    var newUsings = oldUsings;

                    foreach (var usingStatement in classWithTheLongestInheritanceChainUsings)
                    {
                        if (!newUsings.Any(usingDirective => usingDirective.ToString() == usingStatement.ToString()))
                        {
                            newUsings = newUsings.Add(usingStatement);
                        }
                    }

                    var modifiedClass = classToExamine.WithBaseList(
                        newBaseList);

                    _originalToModifiedClassesDictionary[classToExamine].NewClassDeclarationSyntax = modifiedClass;
                    _originalToModifiedClassesDictionary[classToExamine].NewClassUsings = newUsings;

                }
            }

        }

        private List<string> GetInheritanceChain(ClassDeclarationSyntax classToCheck)
        {
            var inheritanceChain = new List<string>();
            var currentBaseClassName = classToCheck.GetBaseClassName();

            while(currentBaseClassName != string.Empty)
            {
                inheritanceChain.Add(currentBaseClassName);

                var currentBaseClass = _originalAllClasses.FirstOrDefault(baseClass => baseClass.Identifier.ValueText == currentBaseClassName);
                if (currentBaseClass == null)
                {
                    currentBaseClassName = string.Empty;
                }
                else
                {
                    currentBaseClassName = currentBaseClass.GetBaseClassName();
                }
            }

            inheritanceChain.Reverse();
            return inheritanceChain;
        }

        private void ElevateSharedLogicToBaseClass(
            List<ClassDeclarationSyntax> classesContainingSharedLogic,
            ClassDeclarationSyntax baseClass,
            CancellationToken cancellationToken)
        {
            ElevateSharedFields(classesContainingSharedLogic, baseClass);
            ElevateSharedProperties(classesContainingSharedLogic, baseClass);
            ElevateSharedMethods(classesContainingSharedLogic, baseClass);
        }

        private void ElevateSharedMethods(List<ClassDeclarationSyntax> classesContainingSharedLogic,
            ClassDeclarationSyntax baseClass)
        {
            var sharedMethods = new List<MethodDeclarationSyntax>();

            var methodsToExamine = classesContainingSharedLogic.First().DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

            foreach (var method in methodsToExamine)
            {
                if (ClassesContainsMethod(classesContainingSharedLogic, method))
                {
                    sharedMethods.Add(method);
                }
            }

            foreach (var classToFix in classesContainingSharedLogic)
            {
                var modifiedClassToFix = _originalToModifiedClassesDictionary[classToFix].NewClassDeclarationSyntax;

                foreach (var sharedMethod in sharedMethods)
                {
                    var methodToRemove = modifiedClassToFix.DescendantNodes()
                        .OfType<MethodDeclarationSyntax>().FirstOrDefault(prop => prop.Identifier.ValueText == sharedMethod.Identifier.ValueText);

                    if (methodToRemove != null)
                    {
                        modifiedClassToFix = modifiedClassToFix.RemoveNode(methodToRemove, SyntaxRemoveOptions.KeepNoTrivia);
                    }
                }

                _originalToModifiedClassesDictionary[classToFix].NewClassDeclarationSyntax = modifiedClassToFix;
            }

            var modifiedBaseClass = _originalToModifiedClassesDictionary[baseClass].NewClassDeclarationSyntax;

            foreach (var sharedMethod in sharedMethods)
            {
                var fixedSharedMethod = sharedMethod;

                if (sharedMethod.Modifiers.Any(modif => modif.ValueText == "private"))
                {
                    var privateModifier = sharedMethod.Modifiers.First(modif => modif.ValueText == "private");
                    fixedSharedMethod = sharedMethod.WithModifiers(sharedMethod.Modifiers.Remove(privateModifier));
                    fixedSharedMethod = fixedSharedMethod.AddModifiers(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                }

                var newTargetBaseClassMembers = modifiedBaseClass.Members.Add(fixedSharedMethod);
                modifiedBaseClass = modifiedBaseClass.WithMembers(newTargetBaseClassMembers);
            }

            _originalToModifiedClassesDictionary[baseClass].NewClassDeclarationSyntax = modifiedBaseClass;
        }

        private bool ClassesContainsMethod(List<ClassDeclarationSyntax> classesToExamine, MethodDeclarationSyntax method)
        {
            foreach (var classToExamine in classesToExamine)
            {
                if (!ClassContainsMethod(classToExamine, method))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ClassContainsMethod(ClassDeclarationSyntax classToExamine, MethodDeclarationSyntax methodToExamine)
        {
            var existingMethod = classToExamine.DescendantNodes()
                .OfType<MethodDeclarationSyntax>().FirstOrDefault(method => method.Identifier.ValueText == methodToExamine.Identifier.ValueText);

            return existingMethod != null;
        }

        private void ElevateSharedFields(List<ClassDeclarationSyntax> classesContainingSharedLogic,
            ClassDeclarationSyntax baseClass)
        {
            var sharedFields = new List<FieldDeclarationSyntax>();

            var fieldsToExamine = classesContainingSharedLogic.First().DescendantNodes().OfType<FieldDeclarationSyntax>().ToList();

            foreach (var field in fieldsToExamine)
            {
                if (ClassesContainsField(classesContainingSharedLogic, field))
                {
                    sharedFields.Add(field);
                }
            }

            foreach (var classToFix in classesContainingSharedLogic)
            {
                var modifiedClassToFix = _originalToModifiedClassesDictionary[classToFix].NewClassDeclarationSyntax;

                foreach (var sharedField in sharedFields)
                {
                    var fieldToRemove = modifiedClassToFix.DescendantNodes()
                        .OfType<FieldDeclarationSyntax>().FirstOrDefault(field => field.Declaration.Variables.ToFullString() == sharedField.Declaration.Variables.ToFullString());

                    if (fieldToRemove != null)
                    {
                        modifiedClassToFix = modifiedClassToFix.RemoveNode(fieldToRemove, SyntaxRemoveOptions.KeepNoTrivia);
                    }
                }

                _originalToModifiedClassesDictionary[classToFix].NewClassDeclarationSyntax = modifiedClassToFix;
            }

            var modifiedBaseClass = _originalToModifiedClassesDictionary[baseClass].NewClassDeclarationSyntax;

            foreach (var sharedField in sharedFields)
            {
                var fixedSharedField = sharedField;
                
                if (sharedField.Modifiers.Any(modif => modif.ValueText == "private"))
                {
                    var privateModifier = sharedField.Modifiers.First(modif => modif.ValueText == "private");
                    fixedSharedField = sharedField.WithModifiers(sharedField.Modifiers.Remove(privateModifier));
                    fixedSharedField = fixedSharedField.AddModifiers(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                }
                var newTargetBaseClassMembers = modifiedBaseClass.Members.Add(fixedSharedField);
                modifiedBaseClass = modifiedBaseClass.WithMembers(newTargetBaseClassMembers);
            }

            _originalToModifiedClassesDictionary[baseClass].NewClassDeclarationSyntax = modifiedBaseClass;
        }

        private void ElevateSharedProperties(List<ClassDeclarationSyntax> classesContainingSharedLogic,
            ClassDeclarationSyntax baseClass)
        {
            var sharedProperties = new List<PropertyDeclarationSyntax>();

            var propertiesToExamine = classesContainingSharedLogic.First().DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();

            foreach (var property in propertiesToExamine)
            {
                if (ClassesContainsProperty(classesContainingSharedLogic, property))
                {
                    sharedProperties.Add(property);
                }
            }

            foreach (var classToFix in classesContainingSharedLogic)
            {
                var modifiedClassToFix = _originalToModifiedClassesDictionary[classToFix].NewClassDeclarationSyntax;

                foreach (var sharedProperty in sharedProperties)
                {
                    var propertyToRemove = modifiedClassToFix.DescendantNodes()
                        .OfType<PropertyDeclarationSyntax>().FirstOrDefault(prop => prop.Identifier.ValueText == sharedProperty.Identifier.ValueText);

                    if (propertyToRemove != null)
                    {
                        modifiedClassToFix = modifiedClassToFix.RemoveNode(propertyToRemove, SyntaxRemoveOptions.KeepNoTrivia);
                    }
                }

                _originalToModifiedClassesDictionary[classToFix].NewClassDeclarationSyntax = modifiedClassToFix;
            }

            var modifiedBaseClass = _originalToModifiedClassesDictionary[baseClass].NewClassDeclarationSyntax;

            foreach (var sharedProperty in sharedProperties)
            {
                var fixedSharedProperty = sharedProperty;

                if (sharedProperty.Modifiers.Any(modif => modif.ValueText == "private"))
                {
                    var privateModifier = sharedProperty.Modifiers.First(modif => modif.ValueText == "private");
                    fixedSharedProperty = sharedProperty.WithModifiers(sharedProperty.Modifiers.Remove(privateModifier));
                    fixedSharedProperty = fixedSharedProperty.AddModifiers(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                }

                var newTargetBaseClassMembers = modifiedBaseClass.Members.Add(fixedSharedProperty);
                modifiedBaseClass = modifiedBaseClass.WithMembers(newTargetBaseClassMembers);
            }

            _originalToModifiedClassesDictionary[baseClass].NewClassDeclarationSyntax = modifiedBaseClass;
        }

        private bool ClassesContainsProperty(List<ClassDeclarationSyntax> classesToExamine, PropertyDeclarationSyntax property)
        {
            foreach (var classToExamine in classesToExamine)
            {
                if (!ClassContainsProperty(classToExamine, property))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ClassContainsProperty(ClassDeclarationSyntax classToExamine, PropertyDeclarationSyntax property)
        {
            var existingProperty = classToExamine.DescendantNodes()
                .OfType<PropertyDeclarationSyntax>().FirstOrDefault(prop => prop.Identifier.ValueText == property.Identifier.ValueText);

            return existingProperty != null;
        }

        private bool ClassesContainsField(List<ClassDeclarationSyntax> classesToExamine, FieldDeclarationSyntax field)
        {
            foreach (var classToExamine in classesToExamine)
            {
                if (!ClassContainsField(classToExamine, field))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ClassContainsField(ClassDeclarationSyntax classToExamine, FieldDeclarationSyntax fieldToExamine)
        {
            var existingField = classToExamine.DescendantNodes()
                .OfType<FieldDeclarationSyntax>().FirstOrDefault(field => field.Declaration.Variables.ToFullString() == fieldToExamine.Declaration.Variables.ToFullString());

            return existingField != null;
        }

        private void AddPropertyToBaseClass(
            ref Solution solution,
            ClassDeclarationSyntax targetBaseClass,
            PropertyDeclarationSyntax propertyToAdd)
        {
            var newTargetBaseClassMembers = targetBaseClass.Members.Add(propertyToAdd);
            var newtargetBaseClass = targetBaseClass.WithMembers(newTargetBaseClassMembers);

            var document = solution.GetDocumentContainingNode(targetBaseClass);
            if (document == null)
            {
                return;
            }

            var documentRoot = document.GetSyntaxRootAsync().GetAwaiter().GetResult();

            var oldBaseClass = documentRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault(classDeclaration =>
                classDeclaration.Identifier.ValueText == targetBaseClass.Identifier.ValueText);

            var root = documentRoot.ReplaceNode(oldBaseClass, newtargetBaseClass);
            solution = solution.WithDocumentSyntaxRoot(document.Id, root);
        }

        private void RemovePropertiesFromChildClasses(
            ref Solution solution,
            List<PropertyDeclarationSyntax> propertiesToElevate)
        {
            var treesToModifyDictionary = new Dictionary<SyntaxTree, List<SyntaxNode>>();

            foreach (var propertyToElevate in propertiesToElevate)
            {
                if (!treesToModifyDictionary.ContainsKey(propertyToElevate.SyntaxTree))
                {
                    treesToModifyDictionary.Add(propertyToElevate.SyntaxTree, new List<SyntaxNode>());
                }

                treesToModifyDictionary[propertyToElevate.SyntaxTree].Add(propertyToElevate);
            }

            foreach (var treeToModify in treesToModifyDictionary)
            {
                var originalRoot = treeToModify.Key.GetRoot();
                var root = originalRoot.RemoveNodes(treeToModify.Value, SyntaxRemoveOptions.KeepNoTrivia);
                solution = solution.WithDocumentSyntaxRoot(solution.GetDocumentId(treeToModify.Key), root);
            }
        }
    }
}

