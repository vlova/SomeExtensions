using Microsoft.CodeAnalysis;

namespace SomeExtensions.Extensions.Roslyn {
	public static class SemanticModelExtensions {
		public static ITypeSymbol GetTypeSymbol(this SemanticModel semanticModel, SyntaxNode type) {
			return semanticModel
				.GetSpeculativeTypeInfo(type.SpanStart, type,  SpeculativeBindingOption.BindAsTypeOrNamespace)
				.Type;
		}

		public static ITypeSymbol GetExpressionType(this SemanticModel semanticModel, SyntaxNode type) {
			return semanticModel
				.GetSpeculativeTypeInfo(type.SpanStart, type, SpeculativeBindingOption.BindAsExpression)
				.Type;
		}
	}
}
