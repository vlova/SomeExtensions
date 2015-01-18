using Microsoft.CodeAnalysis;

namespace SomeExtensions.Extensions {
	public static class SemanticModelExtensions {
		public static ITypeSymbol GetSpeculativeTypeSymbol(
			this SemanticModel semanticModel,
			SyntaxNode type,
			SpeculativeBindingOption binding = SpeculativeBindingOption.BindAsTypeOrNamespace) {
			return semanticModel
				.GetSpeculativeTypeInfo(
					type.SpanStart,
					type,
					binding)
				.Type;
		}
	}
}
