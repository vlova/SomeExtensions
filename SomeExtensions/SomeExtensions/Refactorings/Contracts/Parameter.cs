using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Refactorings.Contracts {
	struct ContractParameter {
		public ContractParameter(string name, ExpressionSyntax expression, ITypeSymbol type) {
			Name = name;
			Expression = expression;
			Type = type;
		}

		public string Name { get; }

		public ExpressionSyntax Expression { get; }

		public ITypeSymbol Type { get; }
	}
}
