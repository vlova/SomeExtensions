using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Semantic;

using static Microsoft.CodeAnalysis.SpecialType;

namespace SomeExtensions.Refactorings.UseBaseType {
	internal static class Helpers {
		public static readonly string[] BadSystemTypes = new[] {
			"IComparable",
			"ICloneable",
			"IEquatable",
			"IConvertible",
			"IFormattable",
			"ISerializable"
		};

		public static readonly SpecialType[] BadSpecialTypes = new[] {
			System_Object,
			System_Enum,
			System_Delegate,
			System_MulticastDelegate,
			System_ValueType
		};

		public static ITypeSymbol GetTypeSymbol(ExpressionSyntax node, SemanticModel semanticModel) {
			ITypeSymbol typeSymbol;

			if (node.As<IdentifierNameSyntax>()?.Identifier.Text == "var") {
				node = node.Parent.As<VariableDeclarationSyntax>()
					?.Variables.FirstOrDefault()
					?.Initializer?.Value;

				typeSymbol = semanticModel.GetExpressionType(node);
			}
			else {
				typeSymbol = semanticModel.GetTypeSymbol(node);
			}

			return typeSymbol;
		}
	}
}
