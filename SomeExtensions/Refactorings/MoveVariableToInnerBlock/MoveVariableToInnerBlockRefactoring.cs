using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Semantic;
using SomeExtensions.Extensions.Syntax;
using DependencyTree = System.Collections.Generic.Dictionary<Microsoft.CodeAnalysis.SyntaxNode, System.Collections.Generic.List<Microsoft.CodeAnalysis.SyntaxNode>>;

namespace SomeExtensions.Refactorings.MoveVariableToInnerBlock {
	internal class MoveVariableToInnerBlockRefactoring : IRefactoring {
		struct InjectionPoint {
			public StatementSyntax Statement { get; }
			public SyntaxNode ToPoint { get; }

			public InjectionPoint(StatementSyntax statement, SyntaxNode to) {
				Statement = statement;
				ToPoint = to;
			}
		}

		private readonly BlockSyntax _block;
		private readonly SemanticModel _semanticModel;

		public MoveVariableToInnerBlockRefactoring(BlockSyntax block, SemanticModel semanticModel) {
			_block = block;
			_semanticModel = semanticModel;
		}

		public string Description => "Move variable to inner block";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var withChildStatements = _block.Statements.SelectMany(ExpandStatement);
			var dependenciesTree = _semanticModel.GetDependenciesTree(withChildStatements);
			var injections = _block.Statements
				.Select(st => new InjectionPoint(st, PointToInject(st, dependenciesTree)))
				.Where(_ => _.ToPoint != null)
				.ToList()
				.GroupBy(GetTopLevelParent)
				.ToDictionary(_ => _.Key, _ => _.AsEnumerable());

			var newBlock = _block.WithStatements(Inject(injections).ToSyntaxList());

			return root.ReplaceNode(_block, newBlock);
		}

		private StatementSyntax GetTopLevelParent(InjectionPoint injection)
			=> injection.ToPoint.F(GetTopLevelParent);

		private StatementSyntax GetTopLevelParent(SyntaxNode node)
			=> node
				.GetParents()
				.OfType<StatementSyntax>()
				.First(IsTopStatement);

		private IEnumerable<StatementSyntax> Inject(Dictionary<StatementSyntax, IEnumerable<InjectionPoint>> injections) {
			var mustBeInjectedStatements = injections.SelectMany(_ => _.Value).Select(_ => _.Statement);
			var statements = _block.Statements.Except(mustBeInjectedStatements);
			foreach (var statement in statements) {
				if (injections.ContainsKey(statement)) {
					yield return Inject(statement, injections[statement]);
				}
				else {
					yield return statement;
				}
			}
		}

		private StatementSyntax Inject(StatementSyntax intoStatement, IEnumerable<InjectionPoint> injections) {
			if (injections.IsEmpty()) return intoStatement;

			if (intoStatement is BlockSyntax) {
				var block = intoStatement as BlockSyntax;
				injections = injections.OrderBy(_ => _block.Statements.IndexOf(_.Statement)).ToList();

				var blockStatements = block.Statements.ToList();
				foreach (var injection in injections) {
					var insertionIndex = blockStatements.IndexOf(injection.ToPoint);
					if (insertionIndex == -1) // when original point to insert wasn't statement (in case of if without block)
						insertionIndex = 0; // then insert it to start of the block

					blockStatements.Insert(insertionIndex, injection.Statement.Formattify());
				}

				return block
					.WithStatements(blockStatements.ToSyntaxList());
			}
			else if (intoStatement is IfStatementSyntax) {
				var @if = intoStatement as IfStatementSyntax;
				var trueBranch = InjectIntoBranch(@if.Statement, injections);
				var falseBranch = InjectIntoBranch(@if.Else?.Statement, injections);

				return @if
					.WithStatement(trueBranch)
					.WithElse(@if.Else?.WithStatement(falseBranch));
			}

			throw new Exception("WAT? Impossible");
		}

		private StatementSyntax InjectIntoBranch(
			StatementSyntax branch,
			IEnumerable<InjectionPoint> allInjections) {
			var intoBranchInjections = allInjections.Where(p => p.ToPoint.Ancestors().Contains(branch)).ToList();

			if (branch != null && intoBranchInjections.Any()) {
				var block = branch as BlockSyntax ?? branch.ToBlock().Formattify();
				return Inject(branch, intoBranchInjections);
			}
			else {
				return branch;
			}
		}

		private SyntaxNode PointToInject(StatementSyntax statement, DependencyTree depsMap) {
			var injectionPoints = depsMap
				.Where(pair => pair.Value.Contains(statement))
				.Select(p => p.Key);

			if (injectionPoints.Any(IsCondition)) return null;

			var innerBlockPoints = injectionPoints.Where(IsInnerBlockStatement);

			var innerParents = innerBlockPoints.Select(GetTopLevelParent);
			var topParents = injectionPoints
				.Where(IsTopStatement).Cast<StatementSyntax>()
				.Select(st => PointToInject(st, depsMap))
				.Select(GetTopLevelParent);
			var uniqueParents = innerParents.Concat(topParents).Distinct();

			return (uniqueParents.Count() > 1)
				? null
				: innerBlockPoints.First();
		}

		private bool IsTopStatement(SyntaxNode st) => _block.Statements.Contains(st);
		private bool IsInnerBlockStatement(SyntaxNode st) => !IsTopStatement(st);
		private bool IsCondition(SyntaxNode st) => !(st is StatementSyntax);

		private IEnumerable<SyntaxNode> ExpandStatement(SyntaxNode st) {
			if (st is IfStatementSyntax) {
				var @if = st as IfStatementSyntax;
				yield return @if.Condition;

				foreach (var innerStatement in ExpandStatement(@if.Statement))
					yield return innerStatement;

				var @else = @if.Else?.Statement;
				foreach (var innerStatement in ExpandStatement(@else))
					yield return innerStatement;
			}
			else if (st is BlockSyntax) {
				var block = st as BlockSyntax;
				foreach (var innerStatemnt in block.Statements)
					yield return innerStatemnt;
			}
			else if (st != null) {
				yield return st;
			}
		}
	}
}