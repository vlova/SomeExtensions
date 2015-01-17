using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.Contracts {
	public class ContractNotNullRefactoring : IRefactoring {
		private readonly BaseMethodDeclarationSyntax _method;
		private readonly ParameterSyntax _variable;

		public ContractNotNullRefactoring(ParameterSyntax variable, BaseMethodDeclarationSyntax method) {
			_variable = variable;
			_method = method;
		}

		public string Description {
			get {
				return "Contract.Requires(" + _variable.Identifier.Text + " != null);";
			}
		}

		public async Task<SyntaxNode> ComputeRoot(SyntaxNode root, CancellationToken token) {
			return root
				.Fluent(r => ApplyContractRequires(r))
				.Fluent(r => AddUsingDirective(r));
		}

		private SyntaxNode AddUsingDirective(SyntaxNode root) {
			return root
				.As<CompilationUnitSyntax>()
				.AddUsingIfNotExists(typeof(Contract).Namespace);
		}

		private SyntaxNode ApplyContractRequires(SyntaxNode root) {
			var statements = _method.Body.Statements
				.ToList()
				.Fluent(s => s.Insert(FindRequiresInsertPoint(s), GetNewRequiresStatement()));

			return root.ReplaceNode(
				_method.Body,
				_method.Body.WithStatements(statements.ToSyntaxList()));
		}

		private ExpressionStatementSyntax GetNewRequiresStatement() {
			var contract = string.Format("{0}.{1}({2} != null)",
				nameof(Contract),
				nameof(Contract.Requires),
				_variable.Identifier.Text);

			return SyntaxFactory
				.ParseExpression(contract)
				.ToStatement()
				.Nicefy();
		}

		private static int FindRequiresInsertPoint(IList<StatementSyntax> statements) {
			var lastRequiresStatement = statements
				.FindContracts()
				.LastOrDefault(r => r.GetMethodName() == nameof(Contract.Requires))
				?.Parent
				.As<StatementSyntax>();

			return statements.IndexOf(lastRequiresStatement) + 1;
		}
	}
}
