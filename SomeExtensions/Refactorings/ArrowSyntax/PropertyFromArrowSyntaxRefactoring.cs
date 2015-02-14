using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Refactorings.ArrowSyntax {
	internal class PropertyFromArrowSyntaxRefactoring : IRefactoring {
		private readonly PropertyDeclarationSyntax _property;

		public PropertyFromArrowSyntaxRefactoring(PropertyDeclarationSyntax property) {
			Contract.Requires(property != null);

			_property = property;
		}

		public string Description => "Use default declaration syntax";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var statement = _property.ExpressionBody.Expression.ToReturnStatement();
			var getter = AccessorDeclaration(GetAccessorDeclaration, Block(statement));

			return root.ReplaceNode(
				_property,
				_property
					.WithExpressionBody(null)
					.WithAccessorList(AccessorList(new[] { getter }.ToSyntaxList()))
					.WithSemicolon(None.ToToken())
					.Nicefy());
		}
	}
}