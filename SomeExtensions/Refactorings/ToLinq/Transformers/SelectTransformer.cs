using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Transformers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.ToLinq.Transformers {
	internal class SelectTransformer : ITransformer<ForEachStatementSyntax> {
		private readonly ForEachStatementSyntax _foreach;

		public SelectTransformer(ForEachStatementSyntax @foreach) {
			_foreach = @foreach;
		}

		private BlockSyntax _block => _foreach.Statement as BlockSyntax;
		private LocalDeclarationStatementSyntax _local => _block?.Statements.FirstOrDefault() as LocalDeclarationStatementSyntax;
		private VariableDeclaratorSyntax _variable
			=> _local?.Declaration?.Variables.Count == 1
				? _local.Declaration.Variables.FirstOrDefault()
				: null;

		public bool CanTransform(CompilationUnitSyntax root) {
			return _variable != null
				&& _variable.Initializer != null;
		}

		public TransformationResult<ForEachStatementSyntax> Transform(CompilationUnitSyntax root) {
			var newForeach = _foreach
				.F(AddSelect)
				.WithIdentifier(_variable.Identifier)
				.WithStatement(Block(_block.Statements.Skip(1)))
				.Nicefy();

			return root.Transform(_foreach, newForeach);
		}

		private ForEachStatementSyntax AddSelect(ForEachStatementSyntax @foreach) {
			var lambda = SimpleLambdaExpression(
				Parameter(@foreach.Identifier),
				body: _variable.Initializer.Value);

			var newExpression = @foreach.Expression
				.AccessTo("Select")
				.ToInvocation(lambda);

			return @foreach.WithExpression(newExpression);
		}
	}
}