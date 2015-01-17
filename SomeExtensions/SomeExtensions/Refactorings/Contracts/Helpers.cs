using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.Contracts {
	internal static class Helpers {
		public static bool ContainsRequiresNotNull(
			this BaseMethodDeclarationSyntax method,
			string parameterName) {
			return ContainsRequiresNotNull(method.Body.Statements, parameterName);
		}


		public static IEnumerable<InvocationExpressionSyntax> FindContracts(this IEnumerable<StatementSyntax> statements) {
			return statements
				.OfType<ExpressionStatementSyntax>()
				.Select(r => r.Expression)
				.OfType<InvocationExpressionSyntax>()
				.TakeWhile(r => r.GetClassName() == "Contract");
		}

		public static bool ContainsRequiresNotNull(
			IEnumerable<StatementSyntax> statements,
			string parameterName) {
			return statements
				.FindContracts()
				.TakeWhile(r => r.GetMethodName() == "Requires")
				.Where(r => r.ArgumentList.Arguments.Count == 1)
				.Select(r => r.ArgumentList.Arguments.SingleOrDefault())
				.Select(r => r.Expression)
				.OfType<BinaryExpressionSyntax>()
				.Where(r => r.CSharpKind() == SyntaxKind.NotEqualsExpression)
				.Where(r => r.Left.As<IdentifierNameSyntax>()?.Identifier.Text == parameterName)
				.Where(r => r.Right.As<LiteralExpressionSyntax>()?.CSharpKind() == SyntaxKind.NullLiteralExpression)
				.Any();
		}
	}
}
