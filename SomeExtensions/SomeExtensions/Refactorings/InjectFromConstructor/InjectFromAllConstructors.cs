using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
    internal class InjectFromAllConstructors : IRefactoring {
        private readonly FieldDeclarationSyntax _field;

        public string Description {
            get {
                return "Inject from all constructors";
            }
        }

        public InjectFromAllConstructors(FieldDeclarationSyntax field)  {
            _field = field;
        }

        public async Task<SyntaxNode> ComputeRoot(SyntaxNode root, CancellationToken token) {
            var type = _field.Parent as TypeDeclarationSyntax;
            var typeName = type.Identifier.Text;

            var ctorsCount = type.FindConstructors().Count();
            for (int i = 0; i < ctorsCount; i++) {
                token.ThrowIfCancellationRequested();

                type = root.Find().Type(typeName);
                var ctor = type.FindConstructors().Skip(i).First();

                if (Helpers.NeedInject(_field, ctor, token)) {
                    root = await new InjectFromConstructor(_field, ctor).ComputeRoot(root, token);
                }
            }

            return root;
        }
    }
}
