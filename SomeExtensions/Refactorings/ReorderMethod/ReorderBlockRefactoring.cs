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

namespace SomeExtensions.Refactorings.ReorderBlock {
    internal class ReorderBlockRefactoring : IRefactoring {
        private readonly SemanticModel _semanticModel;
        private readonly BlockSyntax _block;

        public ReorderBlockRefactoring(BlockSyntax block, SemanticModel semanticModel) {
            _block = block;
            _semanticModel = semanticModel;
        }

        public string Description => "Reorder block";

        public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
            var dependenciesMap = _semanticModel.GetDependenciesTree(_block.Statements);
            var statements = GetOrderedStatements(dependenciesMap);

            return root.ReplaceNode(_block, _block.WithStatements(statements.Cast<StatementSyntax>().ToSyntaxList()));
        }

        private List<SyntaxNode> GetOrderedStatements(DependencyTree dependenciesMap) {
            var statements = new List<SyntaxNode>();
            OrderStatements(dependenciesMap.Keys, dependenciesMap, statements);
            InsertMissingStatements(statements);
            return statements;
        }

        private void OrderStatements(
            IEnumerable<SyntaxNode> statements,
            DependencyTree dependenciesMap,
            List<SyntaxNode> reorderedStatements) {
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

        private void InsertMissingStatements(List<SyntaxNode> statements) {
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