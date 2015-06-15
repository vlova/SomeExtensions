using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Refactorings.ToLinq.Simplifiers;
using SomeExtensions.Transformers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.ToLinq.Transformers {
	internal class CollectionAddTransformer : ITransformer<LocalDeclarationStatementSyntax> {
		private readonly ForEachStatementSyntax _foreach;

		public CollectionAddTransformer(ForEachStatementSyntax @foreach) {
			_foreach = @foreach;
		}

		private SyntaxList<StatementSyntax> _parentBlockStatements
			=> _foreach.Parent.As<BlockSyntax>()?.Statements
				?? new SyntaxList<StatementSyntax>();

		private LocalDeclarationStatementSyntax _local
			=> _parentBlockStatements.IndexOf(_foreach) == 0
				? null
				: _parentBlockStatements[_parentBlockStatements.IndexOf(_foreach) - 1]
					.As<LocalDeclarationStatementSyntax>();

		private BlockSyntax _block => _foreach?.Statement as BlockSyntax;

		private InvocationExpressionSyntax _invocation {
			get {
				var lastStatement = _block.Statements.Last();

				return lastStatement
					.As<ExpressionStatementSyntax>()?.Expression
					.As<InvocationExpressionSyntax>();
			}
		}

		public bool CanTransform(CompilationUnitSyntax root) {
			if (_block == null || _local == null) return false;
			if (!_local.Declaration.HasOneVariable()) return false;

			var collectionName = _local.Declaration.GetVariableName();
			return _invocation.GetClassName() == collectionName
				&& _invocation.GetMethodName() == "Add"
				&& _invocation.ArgumentList.Arguments.Count == 1;
		}

		public TransformationResult<LocalDeclarationStatementSyntax> Transform(CompilationUnitSyntax root) {
			var variable = _local.Declaration.Variables.Single();
			var newVariable = variable.WithInitializer(variable.Initializer.WithValue(_foreach.F(ToSelect)));
			var newLocal = _local.WithDeclaration(
				_local.Declaration
					.WithVariables(
						newVariable.ItemToSeparatedList()));

			root = root.TrackNodes(_local, _foreach);
			root = root.RemoveNode(root.GetCurrentNode(_foreach), SyntaxRemoveOptions.KeepDirectives);

			return root.Transform(root.GetCurrentNode(_local), newLocal);
		}

		private ExpressionSyntax ToSelect(ForEachStatementSyntax @foreach) {
			var lambda = SimpleLambdaExpression(
				Parameter(@foreach.Identifier),
				body: _invocation.ArgumentList.Arguments.Single().Expression);

			var newExpression = @foreach.Expression
				.AccessTo("Select")
				.ToInvocation(lambda);

			if (SelectIdentitySimplifier.IsIdentityLambda(newExpression)) {
				return @foreach.Expression;
			}

			return newExpression;
		}
	}
}
