using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.Contracts;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
	public class InjectParameter {
		public InjectParameter(SyntaxNode node, string name, TypeSyntax parameterType, TypeDeclarationSyntax declaredType) {
			Contract.Requires(node != null);
			Contract.Requires(name != null);
			Contract.Requires(parameterType != null);

			Node = node;
			Name = name;
			ParameterType = parameterType;
			DeclaredType = declaredType;
		}

		public SyntaxNode Node { get; }

        public string Name { get; }

		public TypeSyntax ParameterType { get; }

		public TypeDeclarationSyntax DeclaredType { get; }
	}
}
