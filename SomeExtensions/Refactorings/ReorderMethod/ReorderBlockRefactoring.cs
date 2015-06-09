using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using DependencyTree = System.Collections.Generic.Dictionary<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax, System.Collections.Generic.List<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax>>;
using Microsoft.CodeAnalysis.CSharp;

namespace SomeExtensions.Refactorings.MakeGeneric {
    internal class ReorderBlockRefactoring : IRefactoring {
        private readonly SemanticModel _semanticModel;
        private readonly BlockSyntax _block;

        public ReorderBlockRefactoring(BlockSyntax block, SemanticModel semanticModel) {
            _block = block;
            _semanticModel = semanticModel;
        }

        public string Description => "Reorder block";

        public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
            var dataFlows = GetDataFlows(_block);
            var dependenciesMap = GetDependenciesMap(dataFlows);
            var statements = GetOrderedStatements(dependenciesMap);

            return root.ReplaceNode(_block, _block.WithStatements(statements.ToSyntaxList()));
        }

        private Dictionary<StatementSyntax, DataFlowAnalysis> GetDataFlows(BlockSyntax block) {
            return block.Statements.ToDictionary(s => s, _semanticModel.AnalyzeDataFlow);
        }

        private DependencyTree GetDependenciesMap(Dictionary<StatementSyntax, DataFlowAnalysis> dataFlows) {
            var dependenciesMap = new DependencyTree();
            var writtenSymbolMap = new Dictionary<ISymbol, StatementSyntax>();
            foreach (var statement in _block.Statements) {
                var dataFlow = dataFlows[statement];
                foreach (var symbol in dataFlow.ReadInside) {
                    if (!writtenSymbolMap.ContainsKey(symbol)) // parameters etc
                        continue;

                    var dependencies = dependenciesMap.TryGet(statement) ?? new List<StatementSyntax>();
                    dependencies.Add(writtenSymbolMap[symbol]);
                    dependenciesMap[statement] = dependencies;
                }

                foreach (var symbol in dataFlow.WrittenInside) {
                    writtenSymbolMap[symbol] = statement;
                }
            }

            return dependenciesMap;
        }

        private List<StatementSyntax> GetOrderedStatements(DependencyTree dependenciesMap) {
            var statements = new List<StatementSyntax>();
            OrderStatements(dependenciesMap.Keys, dependenciesMap, statements);
            InsertMissingStatements(statements);
            return statements;
        }

        private void OrderStatements(
            IEnumerable<StatementSyntax> statements,
            DependencyTree dependenciesMap,
            List<StatementSyntax> reorderedStatements) {
            statements = statements
                .OrderBy(_block.Statements.IndexOf)
                .WhereNot(reorderedStatements.Contains);

            foreach (var statement in statements) {
                if (dependenciesMap.ContainsKey(statement)) {
                    OrderStatements(dependenciesMap[statement], dependenciesMap, reorderedStatements);
                }

                reorderedStatements.Add(statement);
            }
        }

        private void InsertMissingStatements(List<StatementSyntax> statements) {
            var firstStatement = _block.Statements[0];
            if (!statements.Contains(firstStatement))
                statements.Insert(0, firstStatement);

            for (int i = 1; i < _block.Statements.Count; i++) {
                var statement = _block.Statements[i];
                if (statements.Contains(statement))
                    continue;

                var nearestStatement = statements
                    .OrderBy(s => Math.Abs(_block.Statements.IndexOf(s) - i))
                    .FirstOrDefault();

                statements.Insert(statements.IndexOf(nearestStatement) + 1, statement);
            }
        }
    }
}