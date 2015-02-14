using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.ArrowSyntax {
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
				.WithExpressionBody(ArrowExpressionClause(GetExpression()))
				.WithSemicolonToken(SemicolonToken.ToToken())
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