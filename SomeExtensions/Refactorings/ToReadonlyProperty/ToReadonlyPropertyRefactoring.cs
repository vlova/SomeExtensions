using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static System.Diagnostics.Contracts.Contract;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Microsoft.CodeAnalysis.SyntaxRemoveOptions;

namespace SomeExtensions.Refactorings.ToReadonlyProperty {
	internal class ToReadonlyPropertyRefactoring : IRefactoring {
		private readonly PropertyDeclarationSyntax _property;

        public ToReadonlyPropertyRefactoring(PropertyDeclarationSyntax property){
			Requires(property != null);
			_property = property;
        }

        public string Description => "To readonly property with backing field";

        private string FieldName => _property.Identifier.Text.ToFieldName();

        public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root) {
            var propertyName = _property.Identifier.Text;

            var newType = _property.Parent
				.F(n => n.ReplaceNode(_property, CreateProperty()))
				.F(n => n.InsertBefore(n.Find().Property(propertyName), CreateField()));

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
                .F(n => n.ReplaceNode(getAccessor, newGetAccessor))
                .F(n => n.RemoveNode(n.SetAccessor(), KeepNoTrivia))
                .Nicefy();

            return _property.ReplaceNode(_property.AccessorList, newAccessors);
        }
    }
}