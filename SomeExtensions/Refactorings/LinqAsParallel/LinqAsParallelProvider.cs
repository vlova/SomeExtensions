using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Semantic;
using static Microsoft.CodeAnalysis.SpecialType;

namespace SomeExtensions.Refactorings.LinqAsParallel {
	[ExportCodeRefactoringProvider(nameof(LinqAsParallelProvider), LanguageNames.CSharp), Shared]
	internal class LinqAsParallelProvider : BaseRefactoringProvider<InvocationExpressionSyntax> {
		protected override int? FindUpLimit => 5;

		protected override async Task ComputeRefactoringsAsync(RefactoringContext context, InvocationExpressionSyntax invocation) {
			var model = await context.SemanticModelAsync;
			var type = model.GetTypeInfo(invocation).Type;
			if (type == null) return;
			if (type.SpecialType == System_String) return;
			if (!type.IsCollectionType()) return;
			context.RegisterAsync(new LinqAsParallelRefactoring(invocation));
		}
	}
}
