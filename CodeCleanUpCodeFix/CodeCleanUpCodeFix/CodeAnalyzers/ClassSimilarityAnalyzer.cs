using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CodeCleanUpCodeFix.Consts;
using CodeCleanUpCodeFix.Helpers.SyntaxExtensions;
using CodeCleanUpCodeFix.Helpers.SyntaxHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeCleanUpCodeFix.CodeAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ClassSimilarityAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString Title = "Similar class";
        private static readonly LocalizableString MessageFormat = "Class '{0}' is similar to '{1}' - classes match by {2} fields, {3} properties and {4} methods (having {5} lines of code).";
        private static readonly LocalizableString Description = "Logic in similar classes can be unified by extracting a base class";

        private static DiagnosticDescriptor ClassSimilarityRule = new DiagnosticDescriptor(
            DiagnosticsConsts.SimilarClassesDiagnosticId,
            Title,
            MessageFormat,
            IssueCategoryConsts.DuplicateCode,
            DiagnosticSeverity.Warning,
            true,
            Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            ClassSimilarityRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(AnalyzeCompilationForSimilarClasses);
        }

        private void AnalyzeCompilationForSimilarClasses(CompilationAnalysisContext context)
        {
            if(context.Compilation.AssemblyName.Contains("Test"))
            {
                return;
            }

            if (context.Compilation.AssemblyName.Contains("Marketing"))
            {
                var allClasses = context.Compilation.DescendantNodes<ClassDeclarationSyntax>();

                foreach (var currentClass in allClasses)
                {
                    PerformSimilarityAnalysis(context, currentClass, allClasses);
                }
            }
        }

        private void PerformSimilarityAnalysis(
            CompilationAnalysisContext context,
            ClassDeclarationSyntax currentClass,
            List<ClassDeclarationSyntax> allClasses)
        {
            foreach (var currentClassToExamine in allClasses)
            {
                if (currentClass.Identifier.ValueText != currentClassToExamine.Identifier.ValueText)
                {
                    PerformSimilarityAnalysis(context, currentClass, currentClassToExamine);
                }

                //var childClasses = allClasses.Where(classToExamine => classToExamine.IsInheritedFrom(currentClass)).ToList();

                //if (childClasses.Any())
                //{
                //    foreach (var childClass in childClasses)
                //    {
                //        LookForPropertyDuplicationWithinChildClasses(context, childClass, childClasses);
                //    }
                //}
            }
        }

        private void PerformSimilarityAnalysis(
            CompilationAnalysisContext context,
            ClassDeclarationSyntax currentClass,
            ClassDeclarationSyntax potentiallySimilarClass)
        {
            var sameFieldsCount = GetNumberOfSameFields(currentClass, potentiallySimilarClass);
            var samePropertiesCount = GetNumberOfSameProperties(currentClass, potentiallySimilarClass);
            var sameMethodsCount = GetNumberOfSameMethods(currentClass, potentiallySimilarClass);
            var sameMethodsLineCount = GetLineCountOfSameMethods(currentClass, potentiallySimilarClass);

            var sameLineCount = sameFieldsCount + samePropertiesCount * 2 + sameMethodsLineCount;

            if (sameLineCount > 10)
            {
                var diagnostic = Diagnostic.Create(
                    ClassSimilarityRule,
                    currentClass.GetLocation(),
                    new List<Location> { potentiallySimilarClass.GetLocation() },
                    currentClass.Identifier.Value,
                    potentiallySimilarClass.Identifier.ValueText,
                    sameFieldsCount,
                    samePropertiesCount,
                    sameMethodsCount,
                    sameMethodsLineCount);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private int GetNumberOfSameFields(ClassDeclarationSyntax currentClass, ClassDeclarationSyntax potentiallySimilarClass)
        {
            var sameFieldsCount = 0;
            var currentClassFields = currentClass.DescendantNodes().OfType<FieldDeclarationSyntax>().ToList();

            var potentiallySimilarClassFields = potentiallySimilarClass.DescendantNodes().OfType<FieldDeclarationSyntax>().ToList();

            foreach (var field in currentClassFields)
            {
                var equalField = FindEqualFieldInFields(field, potentiallySimilarClassFields);
                if (equalField != null)
                {
                    sameFieldsCount++;
                }
            }

            return sameFieldsCount;
        }



        private int GetNumberOfSameMethods(ClassDeclarationSyntax currentClass, ClassDeclarationSyntax potentiallySimilarClass)
        {
            var sameMethodsCount = 0;
            var currentClassMethods = currentClass.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

            var potentiallySimilarClassMethods = potentiallySimilarClass.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

            foreach (var method in currentClassMethods)
            {
                var equalMethod = FindEqualMethodInMethods(method, potentiallySimilarClassMethods);
                if (equalMethod != null)
                {
                    sameMethodsCount++;
                }
            }

            return sameMethodsCount;
        }

        private int GetLineCountOfSameMethods(ClassDeclarationSyntax currentClass, ClassDeclarationSyntax potentiallySimilarClass)
        {
            var sameMethodsLineCount = 0;
            var currentClassMethods = currentClass.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

            var potentiallySimilarClassMethods = potentiallySimilarClass.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

            foreach (var method in currentClassMethods)
            {
                var equalMethod = FindEqualMethodInMethods(method, potentiallySimilarClassMethods);
                if (equalMethod != null)
                {
                    sameMethodsLineCount+= method.GetText().Lines.Count;
                }
            }

            return sameMethodsLineCount;
        }

        private int GetNumberOfSameProperties(ClassDeclarationSyntax currentClass, ClassDeclarationSyntax potentiallySimilarClass)
        {
            var samePropertiesCount = 0;
            var currentClassProperties = currentClass.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();

            var potentiallySimilarClassProperties = potentiallySimilarClass.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();

                foreach (var property in currentClassProperties)
                {
                    var equalProperty = FindEqualPropertyInProperties(property, potentiallySimilarClassProperties);
                    if (equalProperty != null)
                    {
                        samePropertiesCount++;
                    }
                }

            return samePropertiesCount;
        }

        private FieldDeclarationSyntax FindEqualFieldInFields(
            FieldDeclarationSyntax field,
            List<FieldDeclarationSyntax> fieldsToExamine)
        {
            foreach (var fieldToExamine in fieldsToExamine)
            {
                if (fieldToExamine.Declaration.Variables.ToFullString() == field.Declaration.Variables.ToFullString())
                {
                    // Very simple comparison of methods - could be implemented smarter:
                    return fieldToExamine;
                }
            }

            return null;
        }

        private PropertyDeclarationSyntax FindEqualPropertyInProperties(
            PropertyDeclarationSyntax property,
            List<PropertyDeclarationSyntax> propertiesToExamine)
        {
            foreach (var propertyToExamine in propertiesToExamine)
            {
                if (propertyToExamine.Identifier.ValueText == property.Identifier.ValueText)
                {
                    // Very simple comparison of methods - could be implemented smarter:
                    if (propertyToExamine.GetText().ToString() == property.GetText().ToString())
                    {
                        return propertyToExamine;
                    }
                }
            }

            return null;
        }

        private MethodDeclarationSyntax FindEqualMethodInMethods(
            MethodDeclarationSyntax method,
            List<MethodDeclarationSyntax> methodsToExamine)
        {
            foreach (var methodToExamine in methodsToExamine)
            {
                if (methodToExamine.Identifier.ValueText == method.Identifier.ValueText)
                {
                    // Very simple comparison of methods - could be implemented smarter:
                    if (methodToExamine.Body.GetText().ToString() == method.Body.GetText().ToString())
                    {
                        return methodToExamine;
                    }
                }
            }

            return null;
        }
    }
}
