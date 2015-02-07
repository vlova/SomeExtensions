using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Extensions.Semantic;

using System.Collections.Generic;
using System.Linq;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.UsingStatic {
	internal class UsingStaticRefactoring : IRefactoring {
		private ITypeSymbol _symbol;
		private readonly MemberAccessExpressionSyntax _memberAccess;
		private bool _fixAll;

		public UsingStaticRefactoring(MemberAccessExpressionSyntax memberAccess, ITypeSymbol symbol, bool fixAll) {
			_memberAccess = memberAccess;
			_symbol = symbol;
			_fixAll = fixAll;
		}

		public string Description => "Add using static directive".If(_fixAll, s => s + " (fix all)");

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
			var nodes = GetNodes(root);

			return root
				.ReplaceNodes(nodes, (_, memberAccess) => memberAccess.Name.Nicefy())
				.ReplaceNode(_memberAccess, _memberAccess.Name.Nicefy())
				.As<CompilationUnitSyntax>()
				.AddUsingIfNotExists(_symbol.GetFullName(), @static: true);
		}

		private IEnumerable<MemberAccessExpressionSyntax> GetNodes(SyntaxNode root) {
			if (!_fixAll) {
				return new[] { _memberAccess };
			}

			return root
				.DescendantNodes<MemberAccessExpressionSyntax>()
				.Where(r => AreEquivalent(r.Expression, _memberAccess.Expression));
		}
	}
}