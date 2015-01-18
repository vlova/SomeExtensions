using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
	internal class CreateConstructor : IRefactoring {
		private readonly FieldDeclarationSyntax _field;

		public CreateConstructor(FieldDeclarationSyntax field) {
			_field = field;
		}

		public string Description {
			get {
				return "Create new public constructor";
			}
		}

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
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

			return new InjectFromConstructor(newType.Find().Field(fieldName), newCtor)
				.ComputeRoot(newRoot, token);
		}
	}
}