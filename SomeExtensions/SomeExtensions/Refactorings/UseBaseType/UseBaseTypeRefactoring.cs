using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.UseBaseType {
	internal class UseBaseTypeRefactoring : IRefactoring {
		private readonly ExpressionSyntax _typeNode;
		private readonly ITypeSymbol _typeSymbol;

		public UseBaseTypeRefactoring(ExpressionSyntax typeNode, ITypeSymbol typeSymbol) {
			_typeNode = typeNode;
			_typeSymbol = typeSymbol;
		}

		public string Description {
			get {
				return "Use type "
					+ _typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
			}
		}

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
			var newTypeNode = _typeSymbol.ToTypeSyntax().Nicefy();

            return root
				.As<CompilationUnitSyntax>()
				.ReplaceNode(_typeNode, newTypeNode)
				.AddUsingIfNotExists(_typeSymbol.ContainingNamespace.ToDisplayString());
		}
	}
}