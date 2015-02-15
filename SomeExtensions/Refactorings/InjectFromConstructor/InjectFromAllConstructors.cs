using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static System.Diagnostics.Contracts.Contract;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
	internal class InjectFromAllConstructors : IRefactoring {
        private readonly InjectParameter _parameter;

        public InjectFromAllConstructors(InjectParameter parameter)  {
			Requires(parameter != null);
			_parameter = parameter;
		}

		public string Description => "Inject from all constructors";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var typeName = _parameter.DeclaredType.Identifier.Text;

			foreach (var ctor in GetCtors(() => root, typeName).WhileOk(token)) {
				if (Helpers.NeedInject(_parameter, ctor, token)) {
					root = new InjectFromConstructor(_parameter, ctor).ComputeRoot(root, token);
				}
			}

			return root;
		}

		private static IEnumerable<ConstructorDeclarationSyntax> GetCtors(Func<SyntaxNode> root, string name) {
			var ctorsCount = root().Find().Type(name).FindConstructors().Count();
			return Enumerable
				.Range(0, ctorsCount)
				.Select(i => root().Find().Type(name).FindConstructors().At(i));
		}
	}
}
