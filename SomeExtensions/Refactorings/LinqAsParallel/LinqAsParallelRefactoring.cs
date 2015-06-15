using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.LinqAsParallel {
	internal class LinqAsParallelRefactoring : IAsyncRefactoring {
		private readonly InvocationExpressionSyntax _invocation;

		public LinqAsParallelRefactoring(InvocationExpressionSyntax invocation) {
			_invocation = invocation;
		}


		public string Description => "As parallel";

		public async Task<CompilationUnitSyntax> ComputeRoot(CompilationUnitSyntax root) {
			var newInvocation = _invocation;

			if (_invocation.GetClassName() == "Enumerable" && _invocation.GetMethodName() == "Range") {
				newInvocation = newInvocation.WithExpression(
					newInvocation
						.GetMemberAccessExpression()
						.WithExpression("ParallelEnumerable".ParseExpression()));
			}
			else {
				var child = newInvocation.GetMemberAccessExpression()?.Expression;
				newInvocation = newInvocation.WithExpression(
					newInvocation.GetMemberAccessExpression()
					.WithExpression(
						child
						.AccessTo("AsParallel")
						.ToInvocation()));
			}

			return root.ReplaceNode(_invocation, newInvocation.Nicefy());
		}
	}
}