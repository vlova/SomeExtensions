﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.Contracts {
	internal class AddInOutContractRefactoring : IRefactoring {
		private readonly IEnumerable<BlockSyntax> _bodies;
		private readonly ContractParameter _parameter;
		private readonly IContractProvider _provider;
		private readonly ContractKind _contractKind;

		public AddInOutContractRefactoring(
			IEnumerable<BlockSyntax> bodies,
			ContractParameter parameter,
			IContractProvider provider,
			ContractKind contractKind) {
			Contract.Requires(bodies != null);
			Contract.Requires(provider != null);
			Contract.Requires(Enum.IsDefined(typeof(ContractKind), contractKind));

			_bodies = bodies;
			_parameter = parameter;
			_provider = provider;
			_contractKind = contractKind;
        }

		public string Description
			=> _contractKind.Description() + " " + _provider.GetDescription(_parameter);

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			return root
				.Fluent(r => AddContracts(r))
				.Fluent(r => AddUsingDirectives(r));
		}

		private CompilationUnitSyntax AddUsingDirectives(CompilationUnitSyntax root) {
			var newUnit = root
				.AddUsingIfNotExists(typeof(Contract).Namespace);

			return _provider
				.GetImportNamespaces(_parameter)
				.Aggregate(
					newUnit,
					(node, import) => node.AddUsingIfNotExists(import));
		}

		private CompilationUnitSyntax AddContracts(CompilationUnitSyntax root) {
			return root.ReplaceNodes(_bodies, (body, _) => ComputeNewBody(body));
		}

		private SyntaxNode ComputeNewBody(BlockSyntax body) {
			var statements = body
				.Statements
				.ToList()
				.Fluent(s => s.Insert(FindInsertPoint(s), GetContractStatement()));

			return body.WithStatements(statements.ToSyntaxList());
        }

		private ExpressionStatementSyntax GetContractStatement() {
			return $"{nameof(Contract)}.{_contractKind.MethodName()}"
				.ToInvocation(_provider.GetContractRequire(_parameter))
				.ToStatement()
				.Nicefy();
		}

		private static int FindInsertPoint(IList<StatementSyntax> statements) {
			var lastRequiresStatement = statements
				?.FindContractRequires()
				?.LastOrDefault()
				?.Parent
				?.As<StatementSyntax>();

			return statements.IndexOf(lastRequiresStatement) + 1;
		}
	}
}
