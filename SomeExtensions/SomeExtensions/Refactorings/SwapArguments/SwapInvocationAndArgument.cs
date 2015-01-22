using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Roslyn;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.SwapArguments {
	[ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
	public class SwapInvocationAndArgumentProvider : BaseRefactoringProvider {
		public const string RefactoringId = nameof(SwapInvocationAndArgumentProvider);

		protected override async Task ComputeRefactoringsAsync(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
			var invocation = node.FindUp<InvocationExpressionSyntax>();

			if (!(invocation.Expression is MemberAccessExpressionSyntax)) {
				return;
			}

			if (invocation?.ArgumentList?.Arguments.Count != 1) {
				return;
			}


			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);

			var argument = invocation.ArgumentList.Arguments.Single().Expression;
            var argumentType = semanticModel.GetExpressionType(argument);

			var invokingOn = invocation.Expression.As<MemberAccessExpressionSyntax>()?.Expression;
			var invokeType = semanticModel.GetExpressionType(invokingOn);

			if (invokeType != argumentType) {
				return;
			}

			context.RegisterRefactoring(root, new SwapInvocationAndArgument(invocation));
		}
	}

	internal class SwapInvocationAndArgument : IRefactoring {
		private readonly InvocationExpressionSyntax _invocation;

		public SwapInvocationAndArgument(InvocationExpressionSyntax invocation) {
			_invocation = invocation;
		}

		public string Description { get; } = "Swap invocation and argument";

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
			var memberAcesss = _invocation.Expression.As<MemberAccessExpressionSyntax>();
			var argument = _invocation.ArgumentList.Arguments.Single().Expression;

			var newInvocation = _invocation
				.WithExpression(memberAcesss.WithExpression(argument))
				.WithArgumentList(new[] { memberAcesss.Expression }.ToArgumentList())
				.Nicefy();

			return root.ReplaceNode(_invocation, newInvocation);
		}
	}
}
