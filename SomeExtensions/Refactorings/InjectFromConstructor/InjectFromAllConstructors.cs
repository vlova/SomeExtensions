using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
	internal class InjectFromAllConstructors : IRefactoring {
        private readonly InjectParameter _parameter;

        public InjectFromAllConstructors(InjectParameter parameter)  {
            _parameter = parameter;
		}

		public string Description {
			get {
				return "Inject from all constructors";
			}
		}

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
			var typeName = _parameter.DeclaredType.Identifier.Text;

			foreach (var ctor in GetCtors(root, typeName).WhileOk(token)) {
				if (Helpers.NeedInject(_parameter, ctor, token)) {
					root = new InjectFromConstructor(_parameter, ctor).ComputeRoot(root, token);
				}
			}

			return root;
		}

		private static IEnumerable<ConstructorDeclarationSyntax> GetCtors(SyntaxNode root, string name) {
			var type = root.Find().Type(name);
			var ctorsCount = type.FindConstructors().Count();
			return Enumerable
				.Range(0, ctorsCount)
				.Select(i => type.FindConstructors().At(i));
		}
	}
}
