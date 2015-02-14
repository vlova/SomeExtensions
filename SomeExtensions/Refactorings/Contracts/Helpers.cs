using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Refactorings.Contracts.Providers;

namespace SomeExtensions.Refactorings.Contracts {
	internal static class Helpers {
		public static string ContractClassName => "System.Diagnostics.Contracts.Contract";

		public static IContractProvider[] Providers = new IContractProvider[] {
			new NotNullProvider(),
			new StringNotEmptyProvider(),
			new StringNotWhitespaceProvider(),
			new IsPositiveProvider(),
			new EnumIsDefinedProvider(),
			new CollectionIsNotEmptyProvider()
		};

		public static IEnumerable<InvocationExpressionSyntax> FindInvocations(this IEnumerable<StatementSyntax> statements) {
			Contract.Requires(statements != null);

			return statements
				.OfType<ExpressionStatementSyntax>()
				.Select(r => r.Expression)
				.OfType<InvocationExpressionSyntax>();
		}

		private static IEnumerable<InvocationExpressionSyntax> FindPossibleContractMethods(IEnumerable<StatementSyntax> statements, bool hasStaticImport) {
			return statements
				.FindInvocations()
				.Where(r => (hasStaticImport && r.GetClassName() == null) || r.GetClassName() == nameof(Contract));
		}

		public static IEnumerable<InvocationExpressionSyntax> FindContractRequires(
			this IEnumerable<StatementSyntax> statements, bool hasStaticImport) {
			Contract.Requires(statements != null);

			return FindPossibleContractMethods(statements, hasStaticImport)
				.TakeWhile(r => r.GetMethodName() == nameof(Contract.Requires));
		}

		public static IEnumerable<InvocationExpressionSyntax> FindContractEnsures(
			this IEnumerable<StatementSyntax> statements, bool hasStaticImport) {
			Contract.Requires(statements != null);

			return FindPossibleContractMethods(statements, hasStaticImport)
				.Where(r => r.GetMethodName() == nameof(Contract.Ensures));
		}
	}
}
