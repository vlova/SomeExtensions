using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.SpecialType;

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
			if (IsBadDefaultValue(parameter.DefaultValue)) {
				return false;
			}

			if (parameter.Type.SpecialType.In(_numberTypes)) {
				return true;
			}

			return false;
		}

		private static bool IsBadDefaultValue(ExpressionSyntax defaultValue) {
			var isNegative = defaultValue.ParseLong() <= 0;
			var isNull = defaultValue.IsEquivalentToNull();
			return isNegative || isNull;
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
