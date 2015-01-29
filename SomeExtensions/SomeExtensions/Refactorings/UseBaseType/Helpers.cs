using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Roslyn;

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
			SpecialType.System_Object,
			SpecialType.System_Enum,
			SpecialType.System_Delegate,
			SpecialType.System_MulticastDelegate,
			SpecialType.System_ValueType
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
