using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.ConfigureAwait {
	[ExportCodeRefactoringProvider(nameof(ConfigureAwaitProvider), LanguageNames.CSharp)]
	public class ConfigureAwaitProvider : BaseRefactoringProvider<AwaitExpressionSyntax> {
		protected override int? FindUpLimit => 4;

		protected override bool IsGood(AwaitExpressionSyntax node) =>
			node.Expression.As<InvocationExpressionSyntax>().GetMethodName() != nameof(Task.ConfigureAwait);

		protected override void ComputeRefactorings(RefactoringContext context, AwaitExpressionSyntax await) {
			context.Register(new ConfigureAwaitRefactoring(await));
		}
	}
}
