using System.Composition;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.LanguageNames;
using static Microsoft.CodeAnalysis.SymbolKind;

namespace SomeExtensions.Refactorings.AddArgumentName {
	[ExportCodeRefactoringProvider(nameof(AddArgumentNameProvider), CSharp), Shared]
	internal class AddArgumentNameProvider : BaseRefactoringProvider<ArgumentSyntax> {
		protected override int? FindUpLimit => 4;

		protected async override Task ComputeRefactoringsAsync(RefactoringContext context, ArgumentSyntax argument) {
			if (argument.NameColon != null) return;
			var model = await context.GetSemanticModelAsync();
			var invocation = argument.Parent.Parent as InvocationExpressionSyntax;
			var symbol = model.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
			if (symbol == null) return;
			if (symbol.Kind != Method) return;

			context.Register(new AddArgumentNameRefactoring(argument, symbol));
		}
	}
}
