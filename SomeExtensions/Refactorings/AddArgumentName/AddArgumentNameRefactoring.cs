using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Semantic;
using SomeExtensions.Extensions.Syntax;
using static System.Diagnostics.Contracts.Contract;
using static System.Math;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.AddArgumentName {
	internal class AddArgumentNameRefactoring : IRefactoring {
		private readonly ArgumentSyntax _argument;
        private readonly IMethodSymbol _methodSymbol;
		private readonly ITypeSymbol _lastArgumentType;
		private readonly int _argumentIndex;

		public AddArgumentNameRefactoring(ArgumentSyntax argument, IMethodSymbol methodSymbol, ITypeSymbol lastArgumentType) {
			Requires(argument != null);
			Requires(methodSymbol != null);
			Requires(lastArgumentType != null);

			_argument = argument;
            _methodSymbol = methodSymbol;
			_lastArgumentType = lastArgumentType;

			// provides support for params arguments
			_argumentIndex = Min(
				_argumentsList.Arguments.IndexOf(_argument),
				_methodSymbol.Parameters.Length - 1);
		}

		public string Description => "Add argument name";

		private ArgumentListSyntax _argumentsList => _argument.Parent as ArgumentListSyntax;

		private SeparatedSyntaxList<ArgumentSyntax> _arguments => _argumentsList.Arguments;

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var newArgumentsList = _argumentsList
				.WithArguments(GetNewArguments()
					.Select(ArgumentWithNameColon)
					.ToSeparatedList())
				.Nicefy();

			return root.ReplaceNode(_argument.Parent, newArgumentsList);
		}

		private ArgumentSyntax ArgumentWithNameColon(ArgumentSyntax argument, int argumentIndex) {
			if (argumentIndex >= _argumentIndex) {
				var parameterName = _methodSymbol.Parameters[argumentIndex].Name;
				return argument.WithNameColon(parameterName);
			}
			else {
				return argument;
			}
		}

		private IEnumerable<ArgumentSyntax> GetNewArguments() {
			var needConvertParams = NeedConvertParams();

			var defaultArgumentsLength = needConvertParams
				? _methodSymbol.Parameters.Length - 1
				: _methodSymbol.Parameters.Length;

			var defaultArguments = _arguments.Take(defaultArgumentsLength);

			if (needConvertParams) {
				var paramsArgs = _arguments.Skip(defaultArgumentsLength)
					.Select(r => r.Expression)
					.ToList();

				if (paramsArgs.Any()) {
					var lastArgument = paramsArgs.ToArrayCreation();
					return defaultArguments.Append(Argument(lastArgument));
				}
			}

			return defaultArguments;
		}

		private bool NeedConvertParams() {
			var lastParameter = _methodSymbol.Parameters.Last();
			var elementType = lastParameter.Type.As<IArrayTypeSymbol>()?.ElementType;

            return lastParameter.IsParams
				&& !_lastArgumentType.IsCollectionTypeOf(elementType);
		}
	}
}