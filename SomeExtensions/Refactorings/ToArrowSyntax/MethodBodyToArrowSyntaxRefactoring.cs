using System;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis;
using SomeExtensions.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.ToArrowSyntax {
	internal class MethodBodyToArrowSyntaxRefactoring : IRefactoring {
		private readonly MethodDeclarationSyntax _method;

		public MethodBodyToArrowSyntaxRefactoring(MethodDeclarationSyntax method) {
			Contract.Requires(method != null);

			_method = method;
		}

		public string Description => "Use arrow syntax";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var newMethod = _method
				.WithBody(null)
				.WithExpressionBody(SyntaxFactory.ArrowExpressionClause(GetExpression()))
				.WithSemicolonToken(SyntaxKind.SemicolonToken.ToToken())
				.Nicefy();

			return root.ReplaceNode(_method, newMethod);
		}

		private ExpressionSyntax GetExpression() {
			ExpressionSyntax expression;
			var statement = _method.Body.Statements.First();

			if (statement is ReturnStatementSyntax) {
				expression = statement.As<ReturnStatementSyntax>().Expression;
			}
			else {
				expression = statement.As<ExpressionStatementSyntax>().Expression;
			}

			return expression;
		}
	}
}