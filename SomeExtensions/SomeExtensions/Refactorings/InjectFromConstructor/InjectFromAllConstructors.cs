using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
	internal class InjectFromAllConstructors : IRefactoring {
        private readonly InjectParameter _parameter;

        public string Description {
            get {
                return "Inject from all constructors";
            }
        }

        public InjectFromAllConstructors(InjectParameter parameter)  {
            _parameter = parameter;
        }

        public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
            var type = _parameter.DeclaredType;
            var typeName = type.Identifier.Text;

            var ctorsCount = type.FindConstructors().Count();
            for (int i = 0; i < ctorsCount; i++) {
                token.ThrowIfCancellationRequested();

                type = root.Find().Type(typeName);
                var ctor = type.FindConstructors().Skip(i).First();

                if (Helpers.NeedInject(_parameter, ctor, token)) {
                    root = new InjectFromConstructor(_parameter, ctor).ComputeRoot(root, token);
                }
            }

            return root;
        }
    }
}
