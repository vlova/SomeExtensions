using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
    internal class InjectFromAllConstructors : BaseRefactoring {
        private readonly FieldDeclarationSyntax _field;

        public override string Description {
            get {
                return "Inject from all constructors";
            }
        }

        public InjectFromAllConstructors(Document document, FieldDeclarationSyntax field) : base(document) {
            _field = field;
        }

        protected override async Task<SyntaxNode> ComputeRootInternal(SyntaxNode root, CancellationToken token) {
            var type = _field.Parent as TypeDeclarationSyntax;
            var typeName = type.Identifier.Text;

            var ctorsCount = type.FindConstructors().Count();
            for (int i = 0; i < ctorsCount; i++) {
                token.ThrowIfCancellationRequested();

                type = root.Find().Type(typeName);
                var ctor = type.FindConstructors().Skip(i).First();

                if (Helpers.NeedInject(_field, ctor, token)) {
                    root = await new InjectFromConstructor(Document, _field, ctor).ComputeRoot(root, token);
                }
            }

            return root;
        }
    }
}
