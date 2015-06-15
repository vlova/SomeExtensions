using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis.Host;
using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings {
	public struct RefactoringContext {
		private readonly CodeRefactoringContext _originalContext;

		public RefactoringContext(CodeRefactoringContext originalContext, CompilationUnitSyntax rootNode) {
			Contract.Requires(rootNode != null);

			_originalContext = originalContext;
			Root = rootNode;
		}

		public CompilationUnitSyntax Root { get; }

		public TextSpan Span => _originalContext.Span;
		public Document Document => _originalContext.Document;
		public Project Project => Document.Project;
		public Solution Solution => Project.Solution;
		public Workspace Workspace => Solution.Workspace;

		public Task<SemanticModel> SemanticModelAsync =>
			Document.GetSemanticModelAsync(CancellationTokenExtensions.GetCancellationToken());

		public void Register(CodeAction action) {
			_originalContext.RegisterRefactoring(action);
		}

		public void RegisterAsync(IAsyncRefactoring refactoring) {
			Contract.Requires(refactoring != null);

			Register(GetCodeAction(
				title: refactoring.Description,
				getRoot: (root, c) => refactoring.ComputeRoot(root),
				context: this));
		}

		public void RegisterAsync(ISolutionRefactoring refactoring) {
			Contract.Requires(refactoring != null);
			var solution = Solution;

			Register(
				CodeAction.Create(
					title: refactoring.Title,
					createChangedSolution: (c) => refactoring.ComputeRoot(solution)));
		}

		public void RegisterAsync(IRefactoring refactoring) {
			Contract.Requires(refactoring != null);

			Register(GetCodeAction(
				title: refactoring.Description,
				getRoot: (root, c) => Task.Run(() => refactoring.ComputeRoot(root), c),
				context: this));
		}

		private static CodeAction GetCodeAction(
			string title,
			Func<CompilationUnitSyntax, CancellationToken, Task<CompilationUnitSyntax>> getRoot,
			RefactoringContext context) {
			Contract.Requires(!string.IsNullOrWhiteSpace(title));
			Contract.Requires(getRoot != null);

			return CodeAction.Create(title, async c => {
				CancellationTokenExtensions.SetCancellationToken(context._originalContext.CancellationToken);

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
			Contract.Requires(refactoring != null);

			var context = this;

			try {
				CancellationTokenExtensions.SetCancellationToken(context._originalContext.CancellationToken);

				var newRoot = refactoring.ComputeRoot(context.Root);
				var document = context.Document.WithSyntaxRoot(newRoot);

				if (ProducedEquivalent(context.Root, newRoot)) {
					document = context.Document;
				}

				var codeAction = CodeAction.Create(refactoring.Description, async c => document);
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
			Contract.Requires(root != null);
			Contract.Requires(newRoot != null);

			return ReferenceEquals(root, newRoot)
				|| SyntaxFactory.AreEquivalent(root, newRoot, false);
		}

		public T GetService<T>() where T : IWorkspaceService {
			return Document.Project.Solution.Workspace.Services.GetService<T>();
		}
	}
}
