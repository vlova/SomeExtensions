using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Extensions.Semantic;

namespace SomeExtensions.Refactorings.UsingStatic {
	internal class UsingStaticRefactoring : IRefactoring {
		private ITypeSymbol _symbol;
		private readonly MemberAccessExpressionSyntax _memberAccess;

		public UsingStaticRefactoring(MemberAccessExpressionSyntax memberAccess, ITypeSymbol symbol) {
			_memberAccess = memberAccess;
			_symbol = symbol;
		}

		public string Description => "Add using static directive";

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
			return root
				.ReplaceNode(_memberAccess, _memberAccess.Name.Nicefy())
				.As<CompilationUnitSyntax>()
				.AddUsingIfNotExists(_symbol.GetFullName(), @static: true);
		}
	}
}