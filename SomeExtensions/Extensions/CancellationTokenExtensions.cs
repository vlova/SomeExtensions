using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomeExtensions.Extensions {
	static class CancellationTokenExtensions {
		public static void ThrowOnCancellation()
			=> GetCancellationToken().ThrowIfCancellationRequested();

		public static bool IsCancellationRequested()
			=> GetCancellationToken().IsCancellationRequested;

		public static CancellationToken GetCancellationToken() {
			var data = CallContext.LogicalGetData(nameof(CancellationToken));
			if (data == null) {
				return default(CancellationToken);
			}
			return (CancellationToken)data;
		}

		public static void SetCancellationToken(CancellationToken token) {
			CallContext.LogicalSetData(nameof(CancellationToken), token);
		}
	}
}
