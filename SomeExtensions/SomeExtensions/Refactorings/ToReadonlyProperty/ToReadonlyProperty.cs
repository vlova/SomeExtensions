using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.ToReadonlyProperty {
    internal class ToReadonlyProperty : BaseRefactoring {
        private PropertyDeclarationSyntax _property;

        public ToReadonlyProperty(Document document, PropertyDeclarationSyntax property) : base(document) {
            _property = property;
        }

        public override string Description {
            get {
                return "To readonly property with backing field";
            }
        }

        private string FieldName {
            get {
                return _property.Identifier.Text.ToFieldName();
            }
        }

        protected override async Task<SyntaxNode> ComputeRootInternal(SyntaxNode root, CancellationToken c) {
            var propertyName = _property.Identifier.Text;
            var fieldName = propertyName.ToFieldName();

            var newType = _property.Parent
                .Fluent(c, n => n.ReplaceNode(_property, CreateProperty()))
                .Fluent(c, n => n.InsertBefore(n.Find().Property(propertyName), CreateField()));

            return root.ReplaceNode(_property.Parent, newType);
        }

        private FieldDeclarationSyntax CreateField() {
            return FieldName
                .ToFieldDeclaration(_property.Type)
                .WithModifiers(SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword)
                .WithLeadingTrivia(_property.GetLeadingTrivia())
                .Nicefy();
        }

        private PropertyDeclarationSyntax CreateProperty() {
            var getAccessor = _property.AccessorList.GetAccessor();

            var newGetAccessor = getAccessor
                .WithTrailingTrivia()
                .WithBody(FieldName.ToIdentifier().ToReturnStatement().ToBlock())
                .WithSemicolonToken(default(SyntaxToken));

            var newAccessors = _property.AccessorList
                .Fluent(n => n.ReplaceNode(getAccessor, newGetAccessor))
                .Fluent(n => n.RemoveNode(n.SetAccessor(), SyntaxRemoveOptions.KeepNoTrivia))
                .Nicefy();

            return _property.ReplaceNode(_property.AccessorList, newAccessors);
        }
    }
}