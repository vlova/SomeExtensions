using System;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace SomeExtensions.Refactorings {
    public abstract class BaseRefactoringProvider : CodeRefactoringProvider {
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
            try {
                var root = await context
                    .Document
                    .GetSyntaxRootAsync(context.CancellationToken)
                    .ConfigureAwait(false);

                if (root != null && !context.CancellationToken.IsCancellationRequested) {
                    ComputeRefactorings(context, root, root.FindNode(context.Span));
                    await ComputeRefactoringsAsync(context, root, root.FindNode(context.Span));
                }
            }
            catch (OperationCanceledException ex) {
                throw;
            }
            catch (Exception ex) {
                // TODO: add logging
            }
        }

        protected virtual void ComputeRefactorings(
            CodeRefactoringContext context, 
            SyntaxNode root, 
            SyntaxNode syntaxNode) {
        }

        protected virtual Task ComputeRefactoringsAsync(
            CodeRefactoringContext context,
            SyntaxNode root,
            SyntaxNode node) {
            return Task.Delay(0);
        }
    }
}
