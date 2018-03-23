using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeCleanUpCodeFix.Helpers.SyntaxHelpers
{
    public class MethodStatementHelper
    {
        public static ExpressionStatementSyntax GetMethodInvocationStatementSyntax(
            string methodOwnerName,
            string methodName,
            ArgumentListSyntax argumentList)
        {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(methodOwnerName),
                                SyntaxFactory.IdentifierName(methodName))
                            .WithOperatorToken(
                                SyntaxFactory.Token(SyntaxKind.DotToken)))
                    .WithArgumentList(argumentList));
        }

        public static MethodDeclarationSyntax GetMethodDeclarationCloneWithEmptyBody(MethodDeclarationSyntax methodToModify)
        {
            return methodToModify.WithBody(methodToModify.Body.WithStatements(new SyntaxList<StatementSyntax>()));
        }

        public static ArgumentListSyntax GetInvocationParameterList(SeparatedSyntaxList<ParameterSyntax> methodToModifyParameters)
        {
            var argumentList = SyntaxFactory.ArgumentList();

            for (var i = 0; i < methodToModifyParameters.Count; i++)
            {
                var parameterName = methodToModifyParameters[i].Identifier.ToString();
                var argumentSyntax = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(parameterName));

                argumentList = argumentList.WithArguments(argumentList.Arguments.Add(argumentSyntax));
            }
            return argumentList;
        }

        public static Location GetMethodBodyLocationFromDeclarationLocation(SyntaxNode root, Location declarationLocation)
        {
            var methodDeclarationSyntax = root.FindNode(declarationLocation.SourceSpan) as MethodDeclarationSyntax;
            if (methodDeclarationSyntax != null)
            {
                return methodDeclarationSyntax.Body.GetLocation();
            }
            return null;
        }
    }
}
