using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Microsoft.CodeAnalysis.SyntaxRemoveOptions;

namespace SomeExtensions.Refactorings.ToReadonlyProperty {
	internal class ToReadonlyPropertyRefactoring : IRefactoring {
        private PropertyDeclarationSyntax _property;

        public ToReadonlyPropertyRefactoring(PropertyDeclarationSyntax property){
			Contract.Requires(property != null);
			_property = property;
        }

        public string Description => "To readonly property with backing field";

        private string FieldName => _property.Identifier.Text.ToFieldName();

        public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken c) {
            var propertyName = _property.Identifier.Text;

            var newType = _property.Parent
                .Fluent(c, n => n.ReplaceNode(_property, CreateProperty()))
                .Fluent(c, n => n.InsertBefore(n.Find().Property(propertyName), CreateField()));

            return root.ReplaceNode(_property.Parent, newType);
        }

        private FieldDeclarationSyntax CreateField() {
			var modifiers = new List<SyntaxKind>() { PrivateKeyword, ReadOnlyKeyword };

			if (_property.HasModifier(StaticKeyword)) {
				modifiers.Add(StaticKeyword);
			}

            return FieldName
				.ToFieldDeclaration(_property.Type)
                .WithModifiers(modifiers)
                .WithLeadingTrivia(_property.GetLeadingTrivia())
                .Nicefy();
        }

        private PropertyDeclarationSyntax CreateProperty() {
            var getAccessor = _property.GetAccessor();

            var newGetAccessor = getAccessor
                .WithTrailingTrivia()
                .WithBody(FieldName.ToIdentifierName().ToReturnStatement().ToBlock())
                .WithSemicolonToken(default(SyntaxToken));

            var newAccessors = _property.AccessorList
                .Fluent(n => n.ReplaceNode(getAccessor, newGetAccessor))
                .Fluent(n => n.RemoveNode(n.SetAccessor(), KeepNoTrivia))
                .Nicefy();

            return _property.ReplaceNode(_property.AccessorList, newAccessors);
        }
    }
}