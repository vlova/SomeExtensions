using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
	internal class CreateConstructor : IRefactoring {
		private readonly InjectParameter _parameter;

		public CreateConstructor(InjectParameter parameter) {
			_parameter = parameter;
		}

		public string Description {
			get {
				return "Create new public constructor";
			}
		}

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
			var type = _parameter.DeclaredType as TypeDeclarationSyntax;

			var ctor = SyntaxFactory
				.ConstructorDeclaration(type.Identifier)
				.WithModifiers(SyntaxKind.PublicKeyword)
				.WithBody(SyntaxFactory.Block())
				.Nicefy();

			var newRoot = root.ReplaceNode(type, type.InsertAfter(_parameter.Node, ctor));
			var newType = newRoot.Find().Type(type);
			var newCtor = newType.FindConstructors().First();

			var newMember = newType.Find().Field(_parameter.Name)
				?? (SyntaxNode)newType.Find().Property(_parameter.Name);
            var injectParameter = Helpers.GetInjectParameter(newMember);

			return new InjectFromConstructor(injectParameter, newCtor)
				.ComputeRoot(newRoot, token);
		}
	}
}