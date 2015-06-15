using System.Composition;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Semantic;

using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.SwapArguments {
	[ExportCodeRefactoringProvider(nameof(SwapInvocationAndArgumentProvider), CSharp), Shared]
	internal class SwapInvocationAndArgumentProvider : BaseRefactoringProvider<InvocationExpressionSyntax> {
		protected override int? FindUpLimit => 3;

		protected override bool IsGood(InvocationExpressionSyntax invocation) {
			return (invocation.Expression is MemberAccessExpressionSyntax)
				&& invocation.ArgumentList?.Arguments.Count == 1;
		}

		protected override async Task ComputeRefactoringsAsync(RefactoringContext context, InvocationExpressionSyntax invocation) {
			var semanticModel = await context.SemanticModelAsync;

			var argument = invocation.ArgumentList.Arguments.Single().Expression;
            var argumentType = semanticModel.GetSpeculativeExpressionType(argument);

			var invokingOn = invocation.Expression.As<MemberAccessExpressionSyntax>()?.Expression;
			var invokeType = semanticModel.GetSpeculativeExpressionType(invokingOn);

			if (invokeType != argumentType) {
				return;
			}

			context.Register(new SwapInvocationAndArgumentRefactoring(invocation));
		}
	}
}
