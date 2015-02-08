using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.AddArgumentName {
	internal class AddArgumentNameRefactoring : IRefactoring {
		private readonly ArgumentSyntax _argument;
		private readonly int _argumentIndex;
        private readonly IMethodSymbol _symbol;

		public AddArgumentNameRefactoring(ArgumentSyntax argument, IMethodSymbol symbol) {
			Contract.Requires(argument != null);
			Contract.Requires(symbol != null);

			_argument = argument;
			_argumentIndex = _argumentsList.Arguments.IndexOf(_argument);
            _symbol = symbol;
		}

		public string Description => "Add argument name";

		private ArgumentListSyntax _argumentsList => _argument.Parent as ArgumentListSyntax;

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var newArguments = _argumentsList.Arguments.Select(ComputeNewArgument).ToSeparatedList();
            var newArgumentsList = _argumentsList.WithArguments(newArguments).Nicefy();
            return root.ReplaceNode(_argument.Parent, newArgumentsList);
		}

		private ArgumentSyntax ComputeNewArgument(ArgumentSyntax argument, int argumentIndex) {
			if (argumentIndex >= _argumentIndex) {
				var argumentName = _symbol.Parameters[argumentIndex].Name;
				var newArgument = _argument.WithNameColon(NameColon(argumentName.ToIdentifierName()));
				return newArgument;
			} else {
				return argument;
			}
		}
	}
}