using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Roslyn;
using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.SplitVariableInitializer {
	[ExportCodeRefactoringProvider(nameof(SplitVariableInitializerProvider), CSharp), Shared]
	internal class SplitVariableInitializerProvider : BaseRefactoringProvider {
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
