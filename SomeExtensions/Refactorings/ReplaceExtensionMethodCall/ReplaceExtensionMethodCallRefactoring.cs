using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Syntax;
using System.Diagnostics.Contracts;

namespace SomeExtensions.Refactorings.ReplaceExtensionMethodCall {
	internal class ReplaceExtensionMethodCallRefactoring : IRefactoring {
		private readonly SemanticModel _model;
		private readonly InvocationExpressionSyntax _invocation;
		private readonly IMethodSymbol _symbol;

		public ReplaceExtensionMethodCallRefactoring(SemanticModel model, InvocationExpressionSyntax invocation, IMethodSymbol symbol) {
			Contract.Requires(model != null);
			Contract.Requires(invocation != null);
			Contract.Requires(symbol != null);

			_model = model;
			_invocation = invocation;
			_symbol = symbol;
		}

		public string Description => "Convert to static method call";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var memberAcess = _invocation.GetMemberAccessExpression();
			var newArgs = _invocation.ArgumentList.WithArguments(
				_invocation.ArgumentList.Arguments.Insert(0, SyntaxFactory.Argument(memberAcess.Expression)));

			var newInvocation = _invocation
				.WithExpression(memberAcess.WithExpression(GetTypeName()))
				.WithArgumentList(newArgs)
				.Formattify();

			return root.ReplaceNode(_invocation, newInvocation);
		}

		private IdentifierNameSyntax GetTypeName() {
			return _symbol.ContainingType
				.ToMinimalDisplayString(_model, _invocation.Span.Start)
				.ToIdentifierName();
		}
	}
}