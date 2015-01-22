using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.JoinVariableInitializer {
	internal class JoinVariableInitializerRefactoring : IRefactoring {
		private readonly AssignmentExpressionSyntax _assigment;
		private readonly LocalDeclarationStatementSyntax _local;
		private readonly bool _useVar;

		public JoinVariableInitializerRefactoring(
			LocalDeclarationStatementSyntax local,
			AssignmentExpressionSyntax assigment,
			bool useVar) {
			_local = local;
			_assigment = assigment;
			_useVar = useVar;
		}

		public string Description {
			get {
				return _useVar
					? "Join declaration and assigment (use var)"
					: "Join declaration and assigment";
			}
		}

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
			var codeBlock = _local.Parent.As<BlockSyntax>();
			var variables = _local.Declaration;
			var variable = variables.Variables.First();
			var localIndex = codeBlock.Statements.IndexOf(_local);

			var newVariables = variable
				.WithInitializer(SyntaxFactory.EqualsValueClause(_assigment.Right))
				.ItemToSeparatedList();

			var newLocal = _local
				.WithDeclaration(variables
					.WithVariables(newVariables)
					.WithType(_useVar
						? "var".ToIdentifierName()
						: variables.Type))
				.Nicefy();

			var newCodeBlock = codeBlock.WithStatements(
				codeBlock
					.Statements
					.RemoveAt(localIndex)
					.RemoveAt(localIndex)
					.Insert(localIndex, newLocal));

			return root.ReplaceNode(codeBlock, newCodeBlock);
		}
	}
}
