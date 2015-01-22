using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Roslyn;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.SplitVariableInitializer {
	[ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
	public class SplitVariableInitializerProvider : BaseRefactoringProvider {
		public const string RefactoringId = nameof(SplitVariableInitializerProvider);

		protected override void ComputeRefactorings(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
			var variableDeclaration = node.FindUp<LocalDeclarationStatementSyntax>()?.Declaration;
			if (variableDeclaration?.Variables.Count != 1) {
				return;
			}

			var variable = variableDeclaration.Variables.First();
			if (variable.Initializer == null) {
				return;
			}

			context.RegisterRefactoring(new SplitVariableInitializerRefactoring(
				variableDeclaration,
				context.Document));
		}
	}

}
