using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Extensions.Roslyn;

namespace SomeExtensions.Refactorings.JoinVariableInitializer {
	[ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
	internal class JoinVariableInitializerProvider : BaseRefactoringProvider {
		public const string RefactoringId = nameof(JoinVariableInitializerProvider);

		protected override void ComputeRefactorings(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
			var local = node.FindUp<LocalDeclarationStatementSyntax>();
			var assigment = node.FindUp<AssignmentExpressionSyntax>();

			if (local == null && assigment == null) {
				return;
			}

			local = local ?? FindLocal(assigment);
			assigment = assigment ?? FindAssigment(local);

			var variables = local?.Declaration;
			if (variables?.Variables.Count != 1) {
				return;
			}

			var variable = variables.Variables.First();
			var assignee = assigment?.Left.As<IdentifierNameSyntax>();
			if (assignee?.Identifier.Text != variable.Identifier.Text) {
				return;
			}

			context.RegisterRefactoring(root,
				new JoinVariableInitializerRefactoring(local, assigment, false));

			if (!local.Declaration.Type.IsVar) {
				context.RegisterRefactoring(root,
					new JoinVariableInitializerRefactoring(local, assigment, true));
            }
		}

		private LocalDeclarationStatementSyntax FindLocal(AssignmentExpressionSyntax assigment) {
			var codeBlock = assigment.Parent.Parent.As<BlockSyntax>();
			var localIndex = codeBlock.Statements.IndexOf(assigment.Parent.As<StatementSyntax>());
			if (localIndex <= 0) {
				return null;
			}

			return codeBlock
				?.Statements[localIndex - 1].As<LocalDeclarationStatementSyntax>();
		}

		private static AssignmentExpressionSyntax FindAssigment(LocalDeclarationStatementSyntax local) {
			var codeBlock = local.Parent.As<BlockSyntax>();
			var localIndex = codeBlock.Statements.IndexOf(local);
			if (localIndex >= codeBlock.Statements.Count) {
				return null;
			}

			return codeBlock
				?.Statements[localIndex + 1].As<ExpressionStatementSyntax>()
				?.Expression.As<AssignmentExpressionSyntax>();
		}
	}
}
