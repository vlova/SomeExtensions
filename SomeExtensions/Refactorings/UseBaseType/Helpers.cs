﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Semantic;

using static Microsoft.CodeAnalysis.SpecialType;
using System.Diagnostics.Contracts;

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
			Contract.Requires(node != null);
			Contract.Requires(semanticModel != null);

			ITypeSymbol typeSymbol;

			if (node.As<IdentifierNameSyntax>()?.Identifier.Text == "var") {
				node = node.Parent.As<VariableDeclarationSyntax>()
					?.Variables.FirstOrDefault()
					?.Initializer?.Value;

				typeSymbol = semanticModel.GetSpeculativeExpressionType(node);
			}
			else {
				typeSymbol = semanticModel.GetSpeculativeTypeSymbol(node as TypeSyntax);
			}

			return typeSymbol;
		}
	}
}
