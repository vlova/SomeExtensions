using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions {
	[ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
	internal class ToReadonlyPropertyRefactoring : CodeRefactoringProvider {
		public const string RefactoringId = "ToReadonlyProperty";

		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var node = root.FindNode(context.Span);

			var propertyDeclaration = node.FindPropertyDeclaration();
			if (propertyDeclaration == null) {
				return;
			}

			var accessors = propertyDeclaration.AccessorList.ChildNodes().OfType<AccessorDeclarationSyntax>().ToList();

			if (accessors.Any(a => a.Body != null)) {
				return;
			}

			if (!accessors.Any()) {
				return;
			}

			var action = CodeAction.Create("To readonly property with backing field", c => ToReadonlyPropertyWithBackingField(context.Document, propertyDeclaration, c));
			context.RegisterRefactoring(action);
		}

		private async Task<Document> ToReadonlyPropertyWithBackingField(Document document, PropertyDeclarationSyntax propertyDecl, CancellationToken c) {
			var propertyName = propertyDecl.Identifier.Text;
			var fieldName = propertyName.ToFieldName();

			var getAccessor = propertyDecl.AccessorList.GetAccessor();
			var setAccessor = propertyDecl.AccessorList.SetAccessor();

			var field = SyntaxFactory
				.FieldDeclaration(
					SyntaxFactory.VariableDeclaration(propertyDecl.Type, SyntaxFactory.SeparatedList(new[] { SyntaxFactory.VariableDeclarator(fieldName) })))
				.WithModifiers(SyntaxFactory.TokenList(
					SyntaxFactory.Token(SyntaxKind.PrivateKeyword).WithTrailingWhitespace(),
					SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword).WithTrailingWhitespace()))
				.WithLeadingTrivia(propertyDecl.GetLeadingTrivia())
				.WithTrailingEndline();

			var newGetAccessor = getAccessor
				.WithBody(
					SyntaxFactory
						.Block(SyntaxFactory
							.ReturnStatement(SyntaxFactory.IdentifierName(fieldName).WithLeadingWhitespace())
							.WithWhitespacesAround())
						.WithWhitespacesAround())
				.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None));

			var newAccessorsDecl = propertyDecl.AccessorList
				.Fluent(n => n.ReplaceNode(getAccessor, newGetAccessor))
				.Fluent(n => n.RemoveNode(n.SetAccessor(), SyntaxRemoveOptions.KeepNoTrivia));

			var newPropertyDecl = propertyDecl
				.ReplaceNode(propertyDecl.AccessorList, newAccessorsDecl);

			var newTypeDecl = propertyDecl.Parent
				.Fluent(t => t.ReplaceNode(propertyDecl, newPropertyDecl))
				.Fluent(t => t.InsertNodesBefore(t.PropertyWithName(propertyName), new[] { field }));

			return await document.WithReplacedNode(propertyDecl.Parent, newTypeDecl, c);
		}
	}
}