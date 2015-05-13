using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace SomeExtensions.Refactorings.MakeGeneric
{
    [ExportCodeRefactoringProvider(nameof(ReorderBlockProvider), LanguageNames.CSharp), Shared]
    internal class ReorderBlockProvider : BaseRefactoringProvider<BlockSyntax> {
		protected override int? FindUpLimit => 5;

		protected override async Task ComputeRefactoringsAsync(RefactoringContext context, BlockSyntax block) {
            var locals = block.Statements.OfType<LocalDeclarationStatementSyntax>();
            var localsAreGood = locals.All(l => l?.Declaration?.Variables.IsSingle() ?? false);

            if (locals.Any() && localsAreGood) {
                context.RegisterAsync(new ReorderBlockRefactoring(block, await context.SemanticModelAsync));
            }
		}
	}
}
