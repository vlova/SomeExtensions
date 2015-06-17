using System.Linq;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Refactorings.ToNewTask {
	[ExportCodeRefactoringProvider(nameof(ToNewTaskProvider), LanguageNames.CSharp), Shared]
	internal class ToNewTaskProvider : BaseRefactoringProvider<ExpressionSyntax> {
		protected override bool IsGood(ExpressionSyntax node) {
			var block = node.FindUp<BlockSyntax>();
			if (block == null) return false;

			var method = block.FindUp<MethodDeclarationSyntax>();
			if (method == null) return false;

			return method.HasModifier(AsyncKeyword);
		}

		private bool IsGoodExpression(ExpressionSyntax expression) {
			if (expression is IdentifierNameSyntax) return false;
			if (expression is LiteralExpressionSyntax) return false;

			if (IsInAwait(expression)) return false;
			return true;
		}

		private static bool IsInAwait(ExpressionSyntax node)
			=> node is AwaitExpressionSyntax
			|| node.Parent is AwaitExpressionSyntax;

		protected override void ComputeRefactorings(RefactoringContext context, ExpressionSyntax node) {
			var expressions = node.GetThisAndParents()
				.TakeWhile(_ => _ is ExpressionSyntax)
				.Cast<ExpressionSyntax>()
				.SkipWhile(e => !IsGoodExpression(e))
				.TakeWhile(IsGoodExpression)
				.Select((expr, index) => new { expr, index })
				.Take(3);

			var block = node.FindUp<BlockSyntax>();
			foreach (var pair in expressions) {
				context.Register(new ToNewTaskRefactoring(block, pair.expr, pair.index));
			}
		}
	}
}
