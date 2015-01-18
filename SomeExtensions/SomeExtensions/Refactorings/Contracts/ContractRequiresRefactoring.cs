using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.Contracts {
	internal class ContractRequiresRefactoring : IRefactoring {
		private readonly BaseMethodDeclarationSyntax _method;
		private readonly ContractParameter _parameter;
		private readonly IContractProvider _provider;

		public ContractRequiresRefactoring(BaseMethodDeclarationSyntax method, ContractParameter parameter, IContractProvider provider) {
			_method = method;
			_parameter = parameter;
			_provider = provider;
		}

		public string Description {
			get {
				return "Require " + _provider.GetDescription(_parameter);
			}
		}

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
			return root
				.Fluent(r => ApplyContractRequires(r))
				.Fluent(r => AddUsingDirectives(r));
		}

		private SyntaxNode AddUsingDirectives(SyntaxNode root) {
			var unit = root
				.As<CompilationUnitSyntax>()
				.AddUsingIfNotExists(typeof(Contract).Namespace);

			return _provider
				.GetImportNamespaces(_parameter)
				.Aggregate(
					unit,
					(node, import) => node.AddUsingIfNotExists(import));
		}

		private SyntaxNode ApplyContractRequires(SyntaxNode root) {
			var statements = _method.Body.Statements.ToList()
				.Fluent(s => s.Insert(FindRequiresInsertPoint(s), GetRequiresStatement()));

			return root.ReplaceNode(
				_method.Body,
				_method.Body.WithStatements(statements.ToSyntaxList()));
		}

		private ExpressionStatementSyntax GetRequiresStatement() {
			return "Contract.Requires"
				.ToInvocation(_provider.GetContractRequire(_parameter))
				.ToStatement()
				.Nicefy();
		}

		private static int FindRequiresInsertPoint(IList<StatementSyntax> statements) {
			var lastRequiresStatement = statements
				.FindContractRequires()
				.LastOrDefault()
				?.Parent
				.As<StatementSyntax>();

			return statements.IndexOf(lastRequiresStatement) + 1;
		}
	}
}
