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
			// dunno how to convert bad if-else patterns or if statements without else branch
			// possible that in simple cases we can use default(T) in cases when we have no else branch
			// sample result: var a = cond ? new Ololo(1, 2, 3) : default(Ololo);
			if (@if.Condition == null) return;
			if (@if.Statement == null) return;
			if (@if.Else?.Statement == null) return;

			var whenTrue = GetStatements(@if.Statement);
			var whenFalse = GetStatements(@if.Else.Statement);

			// that's not correct, we can leave alone same leading and trailing statements
			// if they are equivalent, of course
			if (whenTrue.Count() != whenFalse.Count()) return;

			var diffNodes = SyntaxDiff.FindDiffNodes<ExpressionSyntax>(whenTrue, whenFalse).ToList();
			if (!diffNodes.Any()) return;

			// if-else contains statements (and if-else cannot be used as expression)
			// ternary operator contains expressions (and ternary operator can't be used as statement)
			// so if founded diff node is top statement, then we can't produce ternary operator
			if (diffNodes.Any(r => AreExpressionStatements(@if, r))) return;

			// sometimes diff nodes are lvalues and cannot be converted to ternary operator
			if (!diffNodes.All(r => AreRValues(r))) return;

			if (await ContainsIllegalReference(context, diffNodes)) return;

			context.Register(new ToTernaryOperatorRefactoring(@if, diffNodes));
		}

		private bool AreExpressionStatements(IfStatementSyntax @if, NodeDiff<ExpressionSyntax> r) {
			return IsExpressionStatement(@if, r.First) || IsExpressionStatement(@if, r.Second);
		}

		private bool IsExpressionStatement(IfStatementSyntax @if, ExpressionSyntax node) {
			return node.Parent is ExpressionStatementSyntax;
		}

		private bool AreRValues(NodeDiff<ExpressionSyntax> r) {
			return IsRValue(r.First) && IsRValue(r.Second);
		}

		private bool IsRValue(ExpressionSyntax node) {
			// TODO: if I will add at least four cases, then I must rewrite it with autochecker
			if (node.Parent.As<MemberAccessExpressionSyntax>()?.Name == node) return false;
			if (node.Parent.As<AssignmentExpressionSyntax>()?.Left == node) return false;
			return true;
		}

		private async Task<bool> ContainsIllegalReference(RefactoringContext context, List<NodeDiff<ExpressionSyntax>> nodes) {
			var semanticModel = await context.SemanticModelAsync;
			return nodes.Select(t => t.First).Any(n => HasBadKind(semanticModel, n))
				 || nodes.Select(t => t.Second).Any(n => HasBadKind(semanticModel, n));
		}

		private static bool HasBadKind(SemanticModel semanticModel, ExpressionSyntax node) {
			var kind = semanticModel.GetSymbolInfo(node, CancellationTokenExtensions.GetCancellationToken()).Symbol?.Kind;
			return kind.HasValue && kind.Value == SymbolKind.NamedType;
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
