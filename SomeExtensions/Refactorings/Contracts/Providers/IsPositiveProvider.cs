using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.SpecialType;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Refactorings.Contracts.Providers {
	internal class IsPositiveProvider : IContractProvider {
		private static SpecialType[] _numberTypes = new[] {
			System_Int16,
			System_Int32,
			System_Int64,
			System_Single,
			System_UInt16,
			System_UInt32,
			System_UInt64
		};

		public bool CanRefactor(ContractParameter parameter) {
			if (IsBadDefaultValue(parameter)) {
				return false;
			}

			if (parameter.Type.SpecialType.In(_numberTypes)) {
				return true;
			}

			// TODO: check
			if (parameter.Type.SpecialType == System_Nullable_T) {
				var underlyingType = parameter.Type.As<INamedTypeSymbol>()
					?.TypeArguments.FirstOrDefault();

				return underlyingType.SpecialType.In(_numberTypes);
			}

			return false;
		}

		private static bool IsBadDefaultValue(ContractParameter parameter) {
			if (parameter.As<PrefixUnaryExpressionSyntax>()?.CSharpKind() == UnaryMinusExpression) {
				return false;
			}

			var token = parameter.DefaultValue.As<LiteralExpressionSyntax>()?.Token;

			var isNumber = (token?.CSharpKind() == NumericLiteralToken);
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
