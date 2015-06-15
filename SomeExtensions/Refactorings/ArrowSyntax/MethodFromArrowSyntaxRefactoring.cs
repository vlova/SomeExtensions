using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Refactorings.ArrowSyntax {
	internal class MethodFromArrowSyntaxRefactoring : IRefactoring {
		private readonly MethodDeclarationSyntax _method;

		public MethodFromArrowSyntaxRefactoring(MethodDeclarationSyntax method) {
			Contract.Requires(method != null);
			_method = method;
		}

		public string Description => "Use default declaration syntax";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root) {
			StatementSyntax statement = _method.ReturnType.IsVoid()
				? _method.ExpressionBody.Expression.ToStatement()
				: (StatementSyntax)_method.ExpressionBody.Expression.ToReturnStatement();

			return root.ReplaceNode(
				_method,
				_method
					.WithExpressionBody(null)
					.WithBody(Block(statement))
					.WithSemicolonToken(None.ToToken())
					.Nicefy());
		}
	}
}