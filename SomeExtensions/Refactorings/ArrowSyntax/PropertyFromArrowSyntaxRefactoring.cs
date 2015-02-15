using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using static System.Diagnostics.Contracts.Contract;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static SomeExtensions.Extensions.Syntax.SyntaxFactoryExtensions;

namespace SomeExtensions.Refactorings.ArrowSyntax {
	internal class PropertyFromArrowSyntaxRefactoring : IRefactoring {
		private readonly dynamic _property;

		public PropertyFromArrowSyntaxRefactoring(BasePropertyDeclarationSyntax property) {
			Requires(property != null);
			_property = property;
		}

		public string Description => "Use default declaration syntax";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var statement = SyntaxFactoryExtensions.ToReturnStatement(_property.ExpressionBody.Expression) as ReturnStatementSyntax;
			var getter = AccessorDeclaration(GetAccessorDeclaration, Block(statement));

			var newProperty = NodeExtensions.Nicefy(_property
				.WithExpressionBody(null)
				.WithAccessorList(AccessorList(new[] { getter }.ToSyntaxList()))
				.WithSemicolon(None.ToToken()));

			return root.ReplaceNode(_property as SyntaxNode, newProperty as SyntaxNode);
		}
	}
}