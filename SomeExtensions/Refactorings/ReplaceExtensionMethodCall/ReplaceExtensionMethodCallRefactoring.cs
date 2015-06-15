using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using static System.Diagnostics.Contracts.Contract;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.ReplaceExtensionMethodCall {
	internal class ReplaceExtensionMethodCallRefactoring : IRefactoring {
		private readonly SemanticModel _model;
		private readonly InvocationExpressionSyntax _invocation;
		private readonly IMethodSymbol _symbol;

		public ReplaceExtensionMethodCallRefactoring(SemanticModel model, InvocationExpressionSyntax invocation, IMethodSymbol symbol) {
			Requires(model != null);
			Requires(invocation != null);
			Requires(symbol != null);

			_model = model;
			_invocation = invocation;
			_symbol = symbol;
		}

		public string Description => "Convert to static method call";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root) {
			var memberAccess = _invocation.GetMemberAccessExpression();
			var argumentList = _invocation.ArgumentList;
			var newArgs = argumentList.Arguments.Insert(0, Argument(memberAccess.Expression));
			var newArgList = argumentList.WithArguments(newArgs);

			var newInvocation = _invocation
				.WithExpression(memberAccess.WithExpression(GetTypeName()))
				.WithArgumentList(newArgList)
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