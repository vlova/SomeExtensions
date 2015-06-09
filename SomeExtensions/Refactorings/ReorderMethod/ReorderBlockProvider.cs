using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace SomeExtensions.Refactorings.MakeGeneric {
    [ExportCodeRefactoringProvider(nameof(ReorderBlockProvider), LanguageNames.CSharp), Shared]
    internal class ReorderBlockProvider : BaseRefactoringProvider<BlockSyntax> {
        protected override int? FindUpLimit => 5;

        protected override bool IsGood(BlockSyntax block) {
            var locals = block.Statements.OfType<LocalDeclarationStatementSyntax>();
            var localsAreGood = locals.All(l => l?.Declaration?.Variables.IsSingle() ?? false);
            return locals.Any() && localsAreGood && IfsAreGood(block);
        }

        private bool IfsAreGood(BlockSyntax block) {
            var breakingIfs = block
                .DescendantNodes()
                .OfType<IfStatementSyntax>()
                .Where(f => f.DescendantNodes().Any(IsBreakable));

            return breakingIfs.All(@if => IsLastIf(block, @if));
        }

        public static bool IsBreakable(SyntaxNode p) {
            return p is ReturnStatementSyntax
                || p is ContinueStatementSyntax
                || p is BreakStatementSyntax
                || p is YieldStatementSyntax;
        }

        private bool IsLastIf(BlockSyntax block, IfStatementSyntax @if) {
            SyntaxNode node = @if;
            while (node != block) {
                var parent = node.Parent;
                if (parent is BlockSyntax) {
                    var parentBlock = parent as BlockSyntax;
                    var isLast = (parentBlock.Statements.Last() == node);
                    if (!isLast) return false;
                }

                node = parent;
            }

            return true;
        }

        protected override async Task ComputeRefactoringsAsync(RefactoringContext context, BlockSyntax block) {
            context.RegisterAsync(new ReorderBlockRefactoring(block, await context.SemanticModelAsync));
        }
    }
}
