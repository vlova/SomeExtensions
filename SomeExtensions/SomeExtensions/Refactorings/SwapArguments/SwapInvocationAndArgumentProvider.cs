using System.Composition;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Roslyn;
using SomeExtensions.Extensions.Semantic;

using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.SwapArguments {
	[ExportCodeRefactoringProvider(nameof(SwapInvocationAndArgumentProvider), CSharp), Shared]
	internal class SwapInvocationAndArgumentProvider : BaseRefactoringProvider<InvocationExpressionSyntax> {
		protected override int? FindUpLimit => 3;

		protected override async Task ComputeRefactoringsAsync(CodeRefactoringContext context, SyntaxNode root, InvocationExpressionSyntax invocation) {
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

			context.RegisterRefactoring(root, new SwapInvocationAndArgumentRefactoring(invocation));
		}
	}
}
