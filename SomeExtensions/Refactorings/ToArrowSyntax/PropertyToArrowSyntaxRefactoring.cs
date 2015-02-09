using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
				.WithExpressionBody(ArrowExpressionClause(GetExpression()))
				.WithSemicolon(SemicolonToken.ToToken())
				.Nicefy();

			return root.ReplaceNode(_property, newProperty);
		}

		private ExpressionSyntax GetExpression() {
			var statement = _property.GetAccessor().Body.Statements.First();
			return statement.As<ReturnStatementSyntax>().Expression;
		}
	}
}