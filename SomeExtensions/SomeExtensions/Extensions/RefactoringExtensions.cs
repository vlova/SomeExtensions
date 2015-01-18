﻿using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;

using SomeExtensions.Refactorings;

namespace SomeExtensions.Extensions {
    public static class RefactoringExtensions {
        public static void RegisterRefactoring(this CodeRefactoringContext context, IAsyncRefactoring refactoring) {
            var codeAction = CodeAction.Create(refactoring.Description, async c => {
                var root = await context.Document.GetSyntaxRootAsync(c);
                try {
                    var newRoot = await refactoring.ComputeRoot(root, c);
                    return context.Document.WithSyntaxRoot(newRoot);
                }
                catch (OperationCanceledException) {
                    throw;
                }
                catch (Exception) {
                    // TODO: add logging

                    return context.Document;
                }
            });
            context.RegisterRefactoring(codeAction);
		}

		public static void RegisterRefactoring(this CodeRefactoringContext context, IRefactoring refactoring) {
			var codeAction = CodeAction.Create(refactoring.Description, async c => {
				var root = await context.Document.GetSyntaxRootAsync(c);
				try {
					var newRoot = refactoring.ComputeRoot(root, c);
					return context.Document.WithSyntaxRoot(newRoot);
				}
				catch (OperationCanceledException) {
					throw;
				}
				catch (Exception) {
					// TODO: add logging

					return context.Document;
				}
			});
			context.RegisterRefactoring(codeAction);
		}

		public static void RegisterRefactoring(this CodeRefactoringContext context, SyntaxNode root, IRefactoring refactoring) {
			try {
				var newRoot = refactoring.ComputeRoot(root, context.CancellationToken);
				var document = context.Document.WithSyntaxRoot(newRoot);
				var codeAction = CodeAction.Create(refactoring.Description, document);
				context.RegisterRefactoring(codeAction);
            }
			catch (OperationCanceledException) {
				throw;
			}
			catch (Exception) {
				// TODO: add logging
			}
		}
	}
}
