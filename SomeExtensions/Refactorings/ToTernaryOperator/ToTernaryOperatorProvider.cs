using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.ToTernaryOperator {
	[ExportCodeRefactoringProvider(nameof(ToTernaryOperatorProvider), LanguageNames.CSharp), Shared]
	internal class ToTernaryOperatorProvider : BaseRefactoringProvider<IfStatementSyntax> {
		protected override int? FindUpLimit => 2;

		protected override async Task ComputeRefactoringsAsync(RefactoringContext context, IfStatementSyntax @if) {
			if (@if.Condition == null) return;
			if (@if.Statement == null) return;
			if (@if.Else?.Statement == null) return;

			var whenTrue = GetStatements(@if.Statement);
			var whenFalse = GetStatements(@if.Else.Statement);

			if (whenTrue.Count() != whenFalse.Count()) return;

			var diffNodes = SyntaxDiff.FindDiffNodes<ExpressionSyntax>(whenTrue, whenFalse).ToList();

			if (!diffNodes.Any()) return;
			if (!diffNodes.All(r => CanReplace(r))) return;

			if (await ContainsIllegalReference(context, diffNodes)) return;

			context.Register(new ToTernaryOperatorRefactoring(@if, diffNodes));
		}

		private async Task<bool> ContainsIllegalReference(RefactoringContext context, List<NodeDiff<ExpressionSyntax>> nodes) {
			var semanticModel = await context.SemanticModelAsync;
			return nodes.Select(t => t.First).Any(n => HasBadKind(context.CancellationToken, semanticModel, n))
				 || nodes.Select(t => t.Second).Any(n => HasBadKind(context.CancellationToken, semanticModel, n));
		}

		private static bool HasBadKind(CancellationToken token, SemanticModel semanticModel, ExpressionSyntax node) {
			var kind = semanticModel.GetSymbolInfo(node, token).Symbol?.Kind;
			return kind.HasValue && kind.Value.In(SymbolKind.NamedType);
		}

		private bool CanReplace(NodeDiff<ExpressionSyntax> r) {
			return CanReplace(r.First) && CanReplace(r.Second);
		}

		private bool CanReplace(ExpressionSyntax node) {
			// TODO: if I will add at least four cases, then I must rewrite it with autochecker
			if (node.Parent.As<MemberAccessExpressionSyntax>()?.Name == node) return false;
			if (node.Parent.As<AssignmentExpressionSyntax>()?.Left == node) return false;
			return true;
		}

		private IEnumerable<SyntaxNode> GetStatements(StatementSyntax statement) {
			var block = statement as BlockSyntax;
			if (block != null) {
				foreach (var childStatement in block.Statements) {
					yield return childStatement;
				}
			}
			else {
				yield return statement;
			}
		}
	}
}
