using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.SpeculativeBindingOption;
using DependencyTree = System.Collections.Generic.Dictionary<Microsoft.CodeAnalysis.SyntaxNode, System.Collections.Generic.List<Microsoft.CodeAnalysis.SyntaxNode>>;

namespace SomeExtensions.Extensions.Semantic {
	public static class SemanticModelExtensions {
		public static ITypeSymbol GetSpeculativeTypeSymbol(this SemanticModel semanticModel, TypeSyntax type) {
			return semanticModel
				.GetSpeculativeTypeInfo(type.SpanStart, type, BindAsTypeOrNamespace)
				.Type;
		}

		public static ITypeSymbol GetSpeculativeExpressionType(this SemanticModel semanticModel, SyntaxNode expression) {
			return semanticModel
				.GetSpeculativeTypeInfo(expression.Span.End, expression, BindAsExpression)
				.Type;
		}



		public static Dictionary<SyntaxNode, DataFlowAnalysis> GetDataFlows(this SemanticModel semanticModel, IEnumerable<SyntaxNode> statements) {
			return statements.ToDictionary(s => s, semanticModel.AnalyzeDataFlow);
		}

		public static DependencyTree GetDependenciesTree(this SemanticModel semanticModel, IEnumerable<SyntaxNode> statements) {
			return semanticModel
				.GetDataFlows(statements)
				.F(dataFlows => GetDependenciesMap(statements, dataFlows));
		}

		public static DependencyTree GetDependenciesMap(IEnumerable<SyntaxNode> statements, Dictionary<SyntaxNode, DataFlowAnalysis> dataFlows) {
			var dependenciesMap = new DependencyTree();
			var writtenSymbolMap = new Dictionary<ISymbol, SyntaxNode>();
			foreach (var statement in statements) {
				var dataFlow = dataFlows[statement];
				foreach (var symbol in dataFlow.ReadInside) {
					if (!writtenSymbolMap.ContainsKey(symbol)) // parameters etc
						continue;

					var dependencies = dependenciesMap.TryGet(statement) ?? new List<SyntaxNode>();
					dependencies.Add(writtenSymbolMap[symbol]);
					dependenciesMap[statement] = dependencies;
				}

				foreach (var symbol in dataFlow.WrittenInside) {
					writtenSymbolMap[symbol] = statement;
				}
			}

			return dependenciesMap;
		}
	}
}
