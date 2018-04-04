using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CodeCleanUpCodeFix.Consts;
using CodeCleanUpCodeFix.Helpers.SyntaxExtensions;
using CodeCleanUpCodeFix.Helpers.SyntaxHelpers;
using CodeCleanUpCodeFix.Helpers.WinApiMessage.Interfaces;
using CodeCleanUpCodeFix.Helpers.WinApiMessage;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeCleanUpCodeFix.CodeAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DuplicatePropertySameBaseClassAnalyzer : DiagnosticAnalyzer
    {
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
            context.RegisterCompilationAction(AnalyzeCompilationForDuplicateClassProperties);
        }

        private void AnalyzeCompilationForDuplicateClassProperties(CompilationAnalysisContext context)
        {
            var allClasses = context.Compilation.DescendantNodes<ClassDeclarationSyntax>();

            foreach (var currentClass in allClasses)
            {
                var childClasses = allClasses.Where(classToExamine => classToExamine.IsInheritedFrom(currentClass)).ToList();

                if (childClasses.Any())
                {
                    foreach (var childClass in childClasses)
                    {
                        LookForPropertyDuplicationWithinChildClasses(context, childClass, childClasses);
                    }
                }
            }
        }

        private void LookForPropertyDuplicationWithinChildClasses(
            CompilationAnalysisContext context,
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
                            new List<Location> { equalProperty.GetLocation() },
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
    }
}
