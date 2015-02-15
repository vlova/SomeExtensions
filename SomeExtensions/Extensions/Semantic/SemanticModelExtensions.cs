using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.SpeculativeBindingOption;

namespace SomeExtensions.Extensions.Semantic {
	public static class SemanticModelExtensions {
		public static ITypeSymbol GetTypeSymbol(this SemanticModel semanticModel, TypeSyntax type) {
			return semanticModel
				.GetSpeculativeTypeInfo(type.SpanStart, type, BindAsTypeOrNamespace)
				.Type;
		}

		public static ITypeSymbol GetExpressionType(this SemanticModel semanticModel, SyntaxNode expression) {
			return semanticModel
				.GetSpeculativeTypeInfo(expression.Span.End, expression, BindAsExpression)
				.Type;
		}
	}
}
