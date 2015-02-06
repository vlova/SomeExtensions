using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.ToTernaryOperator {
	[ExportCodeRefactoringProvider(nameof(ToIfElseProvider), LanguageNames.CSharp), Shared]
	internal class ToIfElseProvider : BaseRefactoringProvider<ConditionalExpressionSyntax> {
		protected override int? FindUpLimit => 2;

		protected override void ComputeRefactorings(RefactoringContext context, ConditionalExpressionSyntax ternary) {
			var statement = ternary.FindUp<StatementSyntax>();
			if (statement == null) return;
			context.Register(new ToIfElseRefactoring(ternary, statement));
		}
	}
}
