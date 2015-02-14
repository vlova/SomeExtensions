namespace SomeExtensions.Refactorings.Contracts {
	internal enum ContractKind {
		Require,
		Ensure
	}

	internal static class ContractKindHelpers {
		public static string Description(this ContractKind kind) {
			return kind.ToString();
		}

		public static string GetMethodName(this ContractKind kind) {
			if (kind == ContractKind.Require) {
				return "Requires";
			}
			else {
				return "Ensures";
			}
		}
	}
}
