using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Transformers;

namespace SomeExtensions.Refactorings.ToLinq.Aggregators {
	internal abstract class BaseAggregateTransformer : ITransformer<LocalDeclarationStatementSyntax> {
		protected readonly ForEachStatementSyntax _foreach;

		public BaseAggregateTransformer(ForEachStatementSyntax @foreach) {
			_foreach = @foreach;
		}

		private SyntaxList<StatementSyntax> _parentBlockStatements
			=> _foreach.Parent.As<BlockSyntax>()?.Statements ?? new SyntaxList<StatementSyntax>();

		protected LocalDeclarationStatementSyntax _local
			=> _parentBlockStatements.IndexOf(_foreach) == 0
				? null
				: _parentBlockStatements[_parentBlockStatements.IndexOf(_foreach) - 1]
					.As<LocalDeclarationStatementSyntax>();

		private BlockSyntax _block
			=> _foreach?.Statement as BlockSyntax;

		protected StatementSyntax _lastStatement
			=> _block.Statements.Last();

		public bool CanTransform(CompilationUnitSyntax root) {
			if (_block == null || _local == null) return false;
			if (_block.Statements.Count != 1) return false;
			if (!_local.Declaration.HasOneVariable()) return false;

			var variableName = _local.Declaration.GetVariableName();
			return CanTransform(variableName, root);
		}

		protected abstract bool CanTransform(string variableName, CompilationUnitSyntax root);

		public TransformationResult<LocalDeclarationStatementSyntax> Transform(CompilationUnitSyntax root) {
			var variable = _local.Declaration.Variables.Single();
			var newVariable = variable.WithInitializer(variable.Initializer.WithValue(_foreach.F(ToAggregate)));
			var newLocal = _local.WithDeclaration(
				_local.Declaration
					.WithVariables(
						newVariable.ItemToSeparatedList()));

			root = root.TrackNodes(_local, _foreach);
			root = root.RemoveNode(root.GetCurrentNode(_foreach), SyntaxRemoveOptions.KeepDirectives);

			return root.Transform(root.GetCurrentNode(_local), newLocal);
		}

		protected abstract ExpressionSyntax ToAggregate(ForEachStatementSyntax @foreach);
	}
}
