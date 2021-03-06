﻿using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static System.Diagnostics.Contracts.Contract;

namespace SomeExtensions.Refactorings.SwapArguments {
	internal class SwapInvocationAndArgumentRefactoring : IRefactoring {
		private readonly InvocationExpressionSyntax _invocation;

		public SwapInvocationAndArgumentRefactoring(InvocationExpressionSyntax invocation) {
			Requires(invocation != null);
			_invocation = invocation;
		}

		public string Description => "Swap invocation and argument";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root) {
			var memberAcesss = _invocation.Expression.As<MemberAccessExpressionSyntax>();
			var argument = _invocation.ArgumentList.Arguments.Single().Expression;

			var newInvocation = _invocation
				.WithExpression(memberAcesss.WithExpression(argument))
				.WithArgumentList(new[] { memberAcesss.Expression }.ToArgumentList())
				.Nicefy();

			return root.ReplaceNode(_invocation, newInvocation);
		}
	}
}
