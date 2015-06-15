using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using System.Diagnostics.Contracts;

namespace SomeExtensions.Refactorings {
	public abstract class BaseRefactoringProvider<TNode> : CodeRefactoringProvider
		where TNode : SyntaxNode {
		protected virtual int? FindUpLimit => null;

		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext originalContext) {
			CancellationTokenExtensions.SetCancellationToken(originalContext.CancellationToken);

			if (originalContext.Document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles) {
				return;
			}

			try {
                CompilationUnitSyntax root = (await originalContext
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

		protected virtual bool IsGood(TNode node) => true;

		protected virtual TNode GetNode(CodeRefactoringContext context, SyntaxNode root) {
			Contract.Requires(root != null);

			return root
				.FindNode(context.Span)
				.FindUp<TNode>(IsGood, FindUpLimit);
		}

		protected virtual void ComputeRefactorings(
			RefactoringContext context,
			TNode node) {
			Contract.Requires(node != null);
		}

		protected virtual Task ComputeRefactoringsAsync(
			RefactoringContext context,
			TNode node) {
			Contract.Requires(node != null);
			return Task.Delay(0);
		}
	}
}
