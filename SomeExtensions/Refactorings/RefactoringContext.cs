using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SomeExtensions.Refactorings {
	public struct RefactoringContext {
		private readonly CodeRefactoringContext _originalContext;

		public RefactoringContext(CodeRefactoringContext originalContext, CompilationUnitSyntax rootNode) {
			_originalContext = originalContext;
			Root = rootNode;
		}

		public CompilationUnitSyntax Root { get; }

		public Document Document => _originalContext.Document;

		public TextSpan Span => _originalContext.Span;

		public CancellationToken CancellationToken => _originalContext.CancellationToken;

		public bool IsCancellationRequested => CancellationToken.IsCancellationRequested;

		public Task<SemanticModel> GetSemanticModelAsync() {
			return Document.GetSemanticModelAsync(CancellationToken);
		}

		public void RegisterAsync(IAsyncRefactoring refactoring) {
			_originalContext.RegisterRefactoring(GetCodeAction(
				description: refactoring.Description,
				getRoot: (root, c) => refactoring.ComputeRoot(root, c),
				context: this));
		}

		public void RegisterAsync(IRefactoring refactoring) {
			_originalContext.RegisterRefactoring(GetCodeAction(
				description: refactoring.Description,
				getRoot: (root, c) => Task.Run(() => refactoring.ComputeRoot(root, c), c),
				context: this));
		}

		private static CodeAction GetCodeAction(
			string description,
			Func<CompilationUnitSyntax, CancellationToken, Task<CompilationUnitSyntax>> getRoot,
			RefactoringContext context) {
			return CodeAction.Create(description, async c => {
				var root = await context.Document.GetSyntaxRootAsync(c);
				try {
					var newRoot = await getRoot(root as CompilationUnitSyntax, c);

					if (ProducedEquivalent(root, newRoot)) {
						return context.Document;
					}

					return context.Document.WithSyntaxRoot(newRoot);
				}
				catch (OperationCanceledException) {
					throw;
				}
				catch (Exception) {
					if (Settings.Instance.CanThrow) throw;
					// TODO: add logging

					return context.Document;
				}
			});
		}

		public void Register(IRefactoring refactoring) {
			var context = this;

			try {
				var newRoot = refactoring.ComputeRoot(context.Root, context.CancellationToken);
				var document = context.Document.WithSyntaxRoot(newRoot);

				if (ProducedEquivalent(context.Root, newRoot)) {
					document = context.Document;
				}

				var codeAction = CodeAction.Create(refactoring.Description, document);
				_originalContext.RegisterRefactoring(codeAction);
			}
			catch (OperationCanceledException) {
				throw;
			}
			catch (Exception) {
				if (Settings.Instance.CanThrow) throw;
				// TODO: add logging
			}
		}

		// connected issue: https://connect.microsoft.com/VisualStudio/feedback/details/1096761
		private static bool ProducedEquivalent(SyntaxNode root, SyntaxNode newRoot) {
			return ReferenceEquals(root, newRoot) || SyntaxFactory.AreEquivalent(root, newRoot, false);
		}
	}
}
