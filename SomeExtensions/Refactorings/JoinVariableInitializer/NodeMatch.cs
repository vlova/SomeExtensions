using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using System.Diagnostics.Contracts;

namespace SomeExtensions.Refactorings.JoinVariableInitializer {
	internal class NodeMatch {
		public LocalDeclarationStatementSyntax Local { get; private set; }
		public AssignmentExpressionSyntax Assigment { get; private set; }

		public static NodeMatch Find(SyntaxNode node) {
			Contract.Requires(node != null);

			var local = node.FindUp<LocalDeclarationStatementSyntax>();
			var assigment = node.FindUp<AssignmentExpressionSyntax>();

			if (local == null && assigment == null) {
				return null;
			}

			local = local ?? FindLocal(assigment);
			assigment = assigment ?? FindAssigment(local);

			var variables = local?.Declaration;
			if (variables.HasOneVariable()) {
				if (assigment.GetAssigneeName() == variables.GetVariableName()) {
					return new NodeMatch { Local = local, Assigment = assigment };
				}
			}

			return null;
		}

		private static LocalDeclarationStatementSyntax FindLocal(AssignmentExpressionSyntax assigment) {
			var codeBlock = assigment.Parent.Parent.As<BlockSyntax>();

			var localIndex = codeBlock.Statements.IndexOf(assigment.Parent.As<StatementSyntax>());
			if (localIndex == -1 || localIndex == 0) {
				return null;
			}

			return codeBlock?.Statements[localIndex - 1].As<LocalDeclarationStatementSyntax>();
		}

		private static AssignmentExpressionSyntax FindAssigment(LocalDeclarationStatementSyntax local) {
			var codeBlock = local.Parent.As<BlockSyntax>();

			var localIndex = codeBlock.Statements.IndexOf(local);
			if (localIndex == -1 || localIndex >= codeBlock.Statements.Count) {
				return null;
			}

			return codeBlock
				?.Statements[localIndex + 1].As<ExpressionStatementSyntax>()
				?.Expression.As<AssignmentExpressionSyntax>();
		}
	}
}
