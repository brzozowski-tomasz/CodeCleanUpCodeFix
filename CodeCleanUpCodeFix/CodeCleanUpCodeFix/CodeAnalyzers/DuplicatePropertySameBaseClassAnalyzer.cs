using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CodeCleanUpCodeFix.Consts;
using CodeCleanUpCodeFix.Helpers.SyntaxHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeCleanUpCodeFix.CodeAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DuplicatePropertySameBaseClassAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString Title = "Duplicate property";
        private static readonly LocalizableString MessageFormat = "Property '{0}' in '{1}' class is exactly same as in '{2}' class.";
        private static readonly LocalizableString Description = "Both classes have same base class - common properties can be extracted to base class.";

        private static DiagnosticDescriptor DuplicatedPopertySameBaseClassRule = new DiagnosticDescriptor(
            DiagnosticsConsts.DuplicatePropertySameBaseClassDiagnosticId,
            Title,
            MessageFormat,
            IssueCategoryConsts.DuplicateCode,
            DiagnosticSeverity.Warning,
            true,
            Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            DuplicatedPopertySameBaseClassRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeClassDeclarationNodeForSameMethods, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClassDeclarationNodeForSameMethods(SyntaxNodeAnalysisContext context)
        {
            var currentClass = context.ContainingSymbol as INamedTypeSymbol;
            if (currentClass == null)
            {
                return;
            }

            if (currentClass.DeclaringSyntaxReferences.Length != 1)
            {
                return;
            }

            var currentClassSyntax = (ClassDeclarationSyntax) currentClass.DeclaringSyntaxReferences[0].GetSyntax();

            var classVisitor = new ClassVirtualizationVisitor();
            classVisitor.Visit(context.SemanticModel.SyntaxTree.GetRoot());
            var classes = classVisitor.Classes;
            var childClasses = new List<ClassDeclarationSyntax>();

            foreach (var classToExamine in classes)
            {
                if (classToExamine.BaseList != null)
                {
                    foreach (var baseType in classToExamine.BaseList.Types.OfType<SimpleBaseTypeSyntax>())
                    {
                        var baseTypeIdentifier = baseType.Type as IdentifierNameSyntax;
                        if (baseTypeIdentifier != null)
                        {
                            if (baseTypeIdentifier.Identifier.ValueText == currentClassSyntax.Identifier.ValueText)
                            {
                                childClasses.Add(classToExamine);
                            }
                        }
                    }
                }
            }

            if (childClasses.Count > 0)
            {
                foreach (var childClass in childClasses)
                {
                    LookForPropertyDuplicationWithinChildClasses(context, currentClassSyntax, childClass, childClasses);
                }
            }
        }

        private void LookForPropertyDuplicationWithinChildClasses(
            SyntaxNodeAnalysisContext context,
            ClassDeclarationSyntax baseClass,
            ClassDeclarationSyntax currentChildClass,
            List<ClassDeclarationSyntax> childClasses)
        {
            var currentChildClassProperties = currentChildClass.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();

            foreach (var childClass in childClasses)
            {
                if (childClass.Identifier.ValueText == currentChildClass.Identifier.ValueText)
                {
                    continue;
                }

                var properties = childClass.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();
                
                foreach (var property in currentChildClassProperties)
                {
                    var equalProperty = FindEqualPropertyInProperties(property, properties);
                    if (equalProperty != null)
                    {
                        var diagnostic = Diagnostic.Create(
                            DuplicatedPopertySameBaseClassRule,
                            property.GetLocation(),
                            new List<Location> { equalProperty.GetLocation()},
                            property.Identifier.Value,
                            currentChildClass.Identifier.ValueText,
                            childClass.Identifier.ValueText);

                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private PropertyDeclarationSyntax FindEqualPropertyInProperties(
            PropertyDeclarationSyntax property,
            List<PropertyDeclarationSyntax> propertiesToExamine)
        {
            foreach (var propertyToExamine in propertiesToExamine)
            {
                if (propertyToExamine.Identifier.Value == property.Identifier.Value)
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
    }
}
