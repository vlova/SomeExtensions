using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using static System.Diagnostics.Contracts.Contract;

namespace SomeExtensions.Refactorings.ConfigureAwait {
	internal class ConfigureAwaitRefactoring : IRefactoring {
		private readonly AwaitExpressionSyntax _await;

		public ConfigureAwaitRefactoring(AwaitExpressionSyntax await) {
			Requires(await != null);
			_await = await;
		}

		public string Description => "ConfigureAwait(false)";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root) {
			var newExpression = _await.Expression
				.AccessTo(nameof(Task.ConfigureAwait))
				.ToInvocation(false);

			return root.ReplaceNode(_await.Expression, newExpression);
		}
	}
}