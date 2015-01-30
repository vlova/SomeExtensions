using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Roslyn;

using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.SplitVariableInitializer {
	[ExportCodeRefactoringProvider(nameof(SplitVariableInitializerProvider), CSharp), Shared]
	internal class SplitVariableInitializerProvider : BaseRefactoringProvider<LocalDeclarationStatementSyntax> {
		protected override int? FindUpLimit => 3;

		protected override void ComputeRefactorings(CodeRefactoringContext context, SyntaxNode root, LocalDeclarationStatementSyntax localDeclaration) {
			var variableDeclaration = localDeclaration.Declaration;
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
