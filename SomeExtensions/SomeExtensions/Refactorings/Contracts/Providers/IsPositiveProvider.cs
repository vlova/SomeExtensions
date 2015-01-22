using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.Contracts.Providers {
	internal class IsPositiveProvider : IContractProvider {
		private static SpecialType[] _numberTypes = new[] {
			SpecialType.System_Int16,
			SpecialType.System_Int32,
			SpecialType.System_Int64,
			SpecialType.System_Single,
			SpecialType.System_UInt16,
			SpecialType.System_UInt32,
			SpecialType.System_UInt64
		};

		public bool CanRefactor(ContractParameter parameter) {
			if (IsBadDefaultValue(parameter)) {
				return false;
			}

			if (parameter.Type.SpecialType.In(_numberTypes)) {
				return true;
			}

			// TODO: check
			if (parameter.Type.SpecialType == SpecialType.System_Nullable_T) {
				var underlyingType = parameter.Type.As<INamedTypeSymbol>()
					?.TypeArguments.FirstOrDefault();

				return underlyingType.SpecialType.In(_numberTypes);
			}

			return false;
		}

		private static bool IsBadDefaultValue(ContractParameter parameter) {
			if (parameter.As<PrefixUnaryExpressionSyntax>()?.CSharpKind() == SyntaxKind.UnaryMinusExpression) {
				return false;
			}

			var token = parameter.DefaultValue.As<LiteralExpressionSyntax>()?.Token;

			var isNumber = (token?.CSharpKind() == SyntaxKind.NumericLiteralToken);
			var isPositive = (token?.Text?.ParseInteger() > 0);

			return isNumber && !isPositive;
		}

		public ExpressionSyntax GetContractRequire(ContractParameter parameter) {
			return parameter.Expression.ToGreaterThan(0.ToLiteral());
		}

		public string GetDescription(ContractParameter parameter) {
			return string.Format("{0} > 0", parameter.Name);
		}

		public IEnumerable<string> GetImportNamespaces(ContractParameter parameter) {
			yield return nameof(System);
		}
	}
}
