using System;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;

using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings {
	public abstract class BaseRefactoringProvider<TNode> : CodeRefactoringProvider
		where TNode : SyntaxNode {
		protected virtual int? FindUpLimit => null;

		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
			try {
				var root = await context
					.Document
					.GetSyntaxRootAsync(context.CancellationToken)
					.ConfigureAwait(false);

				if (root != null && !context.CancellationToken.IsCancellationRequested) {
					var node = root.FindNode(context.Span).FindUp<TNode>(FindUpLimit);
					if (node != null) {
						ComputeRefactorings(context, root, node);
						await ComputeRefactoringsAsync(context, root, node);
					}
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
			TNode node) {
		}

		protected virtual Task ComputeRefactoringsAsync(
			CodeRefactoringContext context,
			SyntaxNode root,
			TNode node) {
			return Task.Delay(0);
		}
	}
}
