using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions {
	public static class SemanticModelExtensions {
		public static ITypeSymbol GetSpeculativeTypeSymbol(
			this SemanticModel semanticModel,
			TypeSyntax type) {
			return semanticModel
				.GetSpeculativeTypeInfo(
					type.SpanStart,
					type,
					SpeculativeBindingOption.BindAsTypeOrNamespace)
				.Type;
		}
	}
}
