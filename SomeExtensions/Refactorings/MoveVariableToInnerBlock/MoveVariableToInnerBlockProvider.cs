using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.MoveVariableToInnerBlock {
	[ExportCodeRefactoringProvider(nameof(MoveVariableToInnerBlockProvider), LanguageNames.CSharp), Shared]
	internal class MoveVariableToInnerBlockProvider : BaseRefactoringProvider<BlockSyntax> {
		protected override int? FindUpLimit => 5;

		protected override bool IsGood(BlockSyntax block) {
			var locals = block.Statements.OfType<LocalDeclarationStatementSyntax>();
			var localsAreGood = locals.All(l => l?.Declaration?.Variables.IsSingle() ?? false);
			var haveIfs = block.DescendantNodes().OfType<IfStatementSyntax>().Any();
			var haveChildBlocks = block.DescendantNodes().OfType<BlockSyntax>().Any();
			return locals.Any() && localsAreGood
				&& (haveIfs || haveChildBlocks);
		}

		protected override async Task ComputeRefactoringsAsync(RefactoringContext context, BlockSyntax block) {
			context.RegisterAsync(new MoveVariableToInnerBlockRefactoring(block, await context.SemanticModelAsync));
		}
	}
}
