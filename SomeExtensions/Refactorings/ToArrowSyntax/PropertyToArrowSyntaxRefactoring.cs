using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.ToArrowSyntax {
	internal class PropertyToArrowSyntaxRefactoring : IRefactoring {
		private readonly PropertyDeclarationSyntax _property;

		public PropertyToArrowSyntaxRefactoring(PropertyDeclarationSyntax property) {
			Contract.Requires(property != null);

			_property = property;
		}

		public string Description => "Use arrow syntax";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var newProperty = _property
				.WithAccessorList(null)
				.WithExpressionBody(SyntaxFactory.ArrowExpressionClause(GetExpression()))
				.WithSemicolon(SyntaxKind.SemicolonToken.ToToken())
				.Nicefy();

			return root.ReplaceNode(_property, newProperty);
		}

		private ExpressionSyntax GetExpression() {
			var statement = _property.GetAccessor().Body.Statements.First();
			return statement.As<ReturnStatementSyntax>().Expression;
		}
	}
}