using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings {
	public abstract class BaseRefactoringProvider<TNode> : CodeRefactoringProvider
		where TNode : SyntaxNode {
		protected virtual int? FindUpLimit => null;

		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext originalContext) {
			try {
				var root = (await originalContext
					.Document
					.GetSyntaxRootAsync(originalContext.CancellationToken)
					.ConfigureAwait(false))
					.As<CompilationUnitSyntax>();

				if (root != null && !originalContext.CancellationToken.IsCancellationRequested) {
					var node = GetNode(originalContext, root);
					if (node != null) {
						var newContext = new RefactoringContext(originalContext, root);
						ComputeRefactorings(newContext, node);
						await ComputeRefactoringsAsync(newContext, node);
					}
				}
			}
			catch (OperationCanceledException ex) {
				throw;
			}
			catch (Exception ex) {
				if (Settings.Instance.CanThrow) throw;
				// TODO: add logging
			}
		}

		protected virtual TNode GetNode(CodeRefactoringContext context, SyntaxNode root) {
			return root
				.FindNode(context.Span)
				.FindUp<TNode>(FindUpLimit);
		}

		protected virtual void ComputeRefactorings(
			RefactoringContext context,
			TNode node) {
		}

		protected virtual Task ComputeRefactoringsAsync(
			RefactoringContext context,
			TNode node) {
			return Task.Delay(0);
		}
	}
}
