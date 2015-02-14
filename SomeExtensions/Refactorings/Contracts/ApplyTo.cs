using System;

namespace SomeExtensions.Refactorings.Contracts {
	[Flags]
	internal enum ApplyTo {
		Method = 1,
		Getter = 2,
		Setter = 4,
		Everything = Method | Getter | Setter
	}
}
