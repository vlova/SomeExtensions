using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Refactorings.Contracts.Providers;

namespace SomeExtensions.Refactorings.Contracts {
	internal static class Helpers {
		public static IContractProvider[] Providers = new IContractProvider[] {
			new NotNullProvider(),
			new StringNotEmptyProvider(),
			new StringNotWhitespaceProvider(),
			new IsPositiveProvider(),
			new EnumIsDefinedProvider(),
			new CollectionIsNotEmptyProvider()
		};

		public static IEnumerable<InvocationExpressionSyntax> FindInvocations(this IEnumerable<StatementSyntax> statements) {
			return statements
				.OfType<ExpressionStatementSyntax>()
				.Select(r => r.Expression)
				.OfType<InvocationExpressionSyntax>();
		}

		public static IEnumerable<InvocationExpressionSyntax> FindContractRequires(
			this IEnumerable<StatementSyntax> statements) {
			return statements
				.FindInvocations()
				.TakeWhile(r => r.GetClassName() == nameof(Contract))
				.TakeWhile(r => r.GetMethodName() == nameof(Contract.Requires));
		}
	}
}
