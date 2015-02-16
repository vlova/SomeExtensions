using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Semantic;
using static Microsoft.CodeAnalysis.LanguageNames;
using static Microsoft.CodeAnalysis.SymbolKind;

namespace SomeExtensions.Refactorings.AddArgumentName {
	[ExportCodeRefactoringProvider(nameof(AddArgumentNameProvider), CSharp), Shared]
	internal class AddArgumentNameProvider : BaseRefactoringProvider<ArgumentSyntax> {
		protected override int? FindUpLimit => 4;

		protected override bool IsGood(ArgumentSyntax node)
			=> node.NameColon == null;

		protected async override Task ComputeRefactoringsAsync(RefactoringContext context, ArgumentSyntax argument) {
			var model = await context.SemanticModelAsync;

			var agumentList = argument.Parent as ArgumentListSyntax;
			var calleeMethod = agumentList.Parent as ExpressionSyntax;
			var methodSymbol = model.GetSymbolInfo(calleeMethod).Symbol as IMethodSymbol;
			if (methodSymbol == null) return;
			if (methodSymbol.Kind != Method) return;

			var lastArgumentType = model.GetSpeculativeExpressionType(agumentList.Arguments.Last().Expression as ExpressionSyntax);

			context.Register(new AddArgumentNameRefactoring(argument, methodSymbol, lastArgumentType));
		}
	}
}
