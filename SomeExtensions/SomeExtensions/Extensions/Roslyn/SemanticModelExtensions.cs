using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.SpeculativeBindingOption;

namespace SomeExtensions.Extensions.Roslyn {
	public static class SemanticModelExtensions {
		public static ITypeSymbol GetTypeSymbol(this SemanticModel semanticModel, SyntaxNode type) {
			return semanticModel
				.GetSpeculativeTypeInfo(type.SpanStart, type, BindAsTypeOrNamespace)
				.Type;
		}

		public static ITypeSymbol GetExpressionType(this SemanticModel semanticModel, SyntaxNode type) {
			return semanticModel
				.GetSpeculativeTypeInfo(type.SpanStart, type, BindAsExpression)
				.Type;
		}
	}
}
