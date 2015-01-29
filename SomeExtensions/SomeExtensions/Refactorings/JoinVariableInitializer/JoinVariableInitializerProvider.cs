﻿using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using SomeExtensions.Extensions.Roslyn;

namespace SomeExtensions.Refactorings.JoinVariableInitializer {
	[ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
	internal class JoinVariableInitializerProvider : BaseRefactoringProvider {
		public const string RefactoringId = nameof(JoinVariableInitializerProvider);

		protected override void ComputeRefactorings(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
			var match = NodeMatch.Find(node);
			if (match == null) {
				return;
			}

			if (!match.Local.Declaration.Type.IsVar) {
				context.RegisterRefactoring(root,
					new JoinVariableInitializerRefactoring(match.Local, match.Assigment, true));
			}

			context.RegisterRefactoring(root,
				new JoinVariableInitializerRefactoring(match.Local, match.Assigment, false));
		}
	}
}
