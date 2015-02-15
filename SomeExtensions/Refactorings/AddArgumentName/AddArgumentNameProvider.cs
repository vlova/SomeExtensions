using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Semantic;
using static Microsoft.CodeAnalysis.LanguageNames;
using static Microsoft.CodeAnalysis.SymbolKind;

namespace SomeExtensions.Refactorings.AddArgumentName {
	// TODO: support for constructors
	[ExportCodeRefactoringProvider(nameof(AddArgumentNameProvider), CSharp), Shared]
	internal class AddArgumentNameProvider : BaseRefactoringProvider<ArgumentSyntax> {
		protected override int? FindUpLimit => 4;

		protected override bool IsGood(ArgumentSyntax node)
			=> node.NameColon == null;

		protected async override Task ComputeRefactoringsAsync(RefactoringContext context, ArgumentSyntax argument) {
			var model = await context.SemanticModelAsync;
			var invocation = argument.Parent.Parent as InvocationExpressionSyntax;
			var methodSymbol = model.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
			if (methodSymbol == null) return;
			if (methodSymbol.Kind != Method) return;

			var lastArgumentType = model.GetSpeculativeExpressionType(invocation.ArgumentList.Arguments.Last().Expression);

			context.Register(new AddArgumentNameRefactoring(argument, methodSymbol, lastArgumentType));
		}
	}
}
