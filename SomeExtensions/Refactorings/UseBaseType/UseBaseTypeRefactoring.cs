using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.SymbolDisplayFormat;
using static System.Diagnostics.Contracts.Contract;

namespace SomeExtensions.Refactorings.UseBaseType {
	internal class UseBaseTypeRefactoring : IRefactoring {
		private readonly ExpressionSyntax _typeNode;
		private readonly ITypeSymbol _typeSymbol;

		public UseBaseTypeRefactoring(ExpressionSyntax typeNode, ITypeSymbol typeSymbol) {
			Requires(typeNode != null);
			Requires(typeSymbol != null);

			_typeNode = typeNode;
			_typeSymbol = typeSymbol;
		}

		public string Description =>
			"Use type " + _typeSymbol.ToDisplayString(MinimallyQualifiedFormat);

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root) {
			var newTypeNode = _typeSymbol.ToTypeSyntax().Nicefy();

            return root
				.ReplaceNode(_typeNode, newTypeNode)
				.AddUsingIfNotExists(_typeSymbol.ContainingNamespace.ToDisplayString());
		}
	}
}