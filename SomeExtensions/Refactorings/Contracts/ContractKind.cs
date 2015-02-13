using System;

namespace SomeExtensions.Refactorings.Contracts {
	internal enum ContractKind {
		Require,
		Ensure
	}

	[Flags]
	internal enum ApplyTo {
		Method = 1,
		Getter = 2,
		Setter = 4,
		Everything = Method | Getter | Setter
	}

	internal static class ContractKindHelpers {
		public static string Description(this ContractKind kind) {
			return kind.ToString();
		}

		public static string MethodName(this ContractKind kind) {
			if (kind == ContractKind.Require) {
				return "Requires";
			}
			else {
				return "Ensures";
			}
		}
	}
}
