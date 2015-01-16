using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
    internal class CreateConstructor : BaseRefactoring {
        private readonly FieldDeclarationSyntax _field;

        public CreateConstructor(Document document, FieldDeclarationSyntax field) : base(document) {
            _field = field;
        }

        public override string Description {
            get {
                return "Create new public constructor";
            }
        }

        protected override async Task<SyntaxNode> ComputeRootInternal(SyntaxNode root, CancellationToken token) {
            var variable = _field.Declaration.Variables.FirstOrDefault();
            var fieldName = variable.Identifier.Text;
            var type = _field.Parent as TypeDeclarationSyntax;

            var ctor = SyntaxFactory
                .ConstructorDeclaration(type.Identifier)
                .WithModifiers(SyntaxKind.PublicKeyword)
                .WithBody(SyntaxFactory.Block())
                .Nicefy();

            var newRoot = root.ReplaceNode(type, type.InsertAfter(_field, ctor));
            var newType = newRoot.Find().Type(type);
            var newCtor = newType.FindConstructors().First();
            
            return await new InjectFromConstructor(Document, newType.Find().Field(fieldName), newCtor)
                .ComputeRoot(newRoot, token);
        }
    }
}