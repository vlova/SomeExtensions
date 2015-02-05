using System.Collections.Generic;
using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.ToTernaryOperator {
	[ExportCodeRefactoringProvider(nameof(ToTernaryOperatorProvider), LanguageNames.CSharp), Shared]
	internal class ToTernaryOperatorProvider : BaseRefactoringProvider<IfStatementSyntax> {
		protected override int? FindUpLimit => 2;

		protected override void ComputeRefactorings(RefactoringContext context, IfStatementSyntax @if) {
			if (@if.Condition == null) return;
			if (@if.Statement == null) return;
			if (@if.Else?.Statement == null) return;

			var diffNode = SyntaxDiff.FindDiffNode<ExpressionSyntax>(
				GetStatements(@if.Statement),
				GetStatements(@if.Else.Statement));
			if (diffNode == null) return;

			context.Register(new ToTernaryOperatorRefactoring(@if, diffNode.Value));
		}

		private IEnumerable<SyntaxNode> GetStatements(StatementSyntax statement) {
			var block = statement as BlockSyntax;
			if (block != null) {
				foreach (var childStatement in block.Statements) {
					yield return childStatement;
				}
			} else {
				yield return statement;
			}
		}
	}
}
