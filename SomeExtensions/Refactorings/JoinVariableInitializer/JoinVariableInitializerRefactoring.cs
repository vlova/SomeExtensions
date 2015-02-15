using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static System.Diagnostics.Contracts.Contract;

namespace SomeExtensions.Refactorings.JoinVariableInitializer {
	internal class JoinVariableInitializerRefactoring : IRefactoring {
		private readonly AssignmentExpressionSyntax _assigment;
		private readonly LocalDeclarationStatementSyntax _local;
		private readonly bool _useVar;

		public JoinVariableInitializerRefactoring(
			LocalDeclarationStatementSyntax local,
			AssignmentExpressionSyntax assigment,
			bool useVar) {
			Requires(local != null);
			Requires(assigment != null);

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

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var codeBlock = _local.Parent.As<BlockSyntax>();
			var localIndex = codeBlock.Statements.IndexOf(_local);

			var newStatements = codeBlock
				.Statements
				.RemoveAt(localIndex) // remove variable declaration
				.RemoveAt(localIndex) // remove variable initialization
				.Insert(localIndex, GetNewLocal());

			return root.ReplaceNode(codeBlock, codeBlock.WithStatements(newStatements));
		}

		private LocalDeclarationStatementSyntax GetNewLocal() {
			var variable = _local.Declaration.Variables.First();
			var newVariable = variable.WithInitializer(_assigment.Right.ToInitializer());

			var newLocal = _local
				.WithDeclaration(_local.Declaration
					.WithVariables(newVariable.ItemToSeparatedList())
					.WithType(GetVariableType(_local.Declaration)))
				.Nicefy();

			return newLocal;
		}

		private TypeSyntax GetVariableType(VariableDeclarationSyntax variables) {
			return _useVar ? "var".ToIdentifierName() : variables.Type;
		}
	}
}
