using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.Contracts;

namespace SomeExtensions.Refactorings.Contracts {
	struct ContractParameter {
		public ContractParameter(string name, ExpressionSyntax expression, ExpressionSyntax defaultValue, ITypeSymbol type) {
			Contract.Requires(!string.IsNullOrWhiteSpace(name));
			Contract.Requires(expression != null);
			Contract.Requires(type != null);

			Name = name;
			Expression = expression;
			DefaultValue = defaultValue;
			Type = type;
		}

		public string Name { get; }

		public ExpressionSyntax Expression { get; }

		public ITypeSymbol Type { get; }

		public ExpressionSyntax DefaultValue { get; }
	}
}
