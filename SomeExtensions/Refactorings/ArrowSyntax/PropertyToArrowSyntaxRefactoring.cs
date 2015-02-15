using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static System.Diagnostics.Contracts.Contract;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Refactorings.ArrowSyntax {
	internal class PropertyToArrowSyntaxRefactoring : IRefactoring {
		private readonly dynamic _property;

		public PropertyToArrowSyntaxRefactoring(BasePropertyDeclarationSyntax property) {
			Requires(property != null);

			_property = property;
		}

		public string Description => "Use arrow syntax";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var newProperty = NodeExtensions.Nicefy(_property
				.WithAccessorList(null)
				.WithExpressionBody(ArrowExpressionClause(GetExpression()))
				.WithSemicolon(SemicolonToken.ToToken()));

			return root.ReplaceNode(_property as SyntaxNode, newProperty as SyntaxNode);
		}

		private ExpressionSyntax GetExpression() {
			var statement = PropertyExtensions.GetAccessor(_property).Body.Statements.First() as StatementSyntax;
			return statement.As<ReturnStatementSyntax>().Expression;
		}
	}
}