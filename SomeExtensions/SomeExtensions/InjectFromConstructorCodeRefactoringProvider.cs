using System;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace SomeExtensions {
	[ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
	internal class InjectFromConstructorRefactoring : CodeRefactoringProvider {
		public const string RefactoringId = "InjectFromConstructor";

		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
			try {
				var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

				if (context.CancellationToken.IsCancellationRequested) {
					return;
				}

				var node = root.FindNode(context.Span);

				var fieldDecl = node.FindFieldDeclaration();

				if (!CanHandle(fieldDecl)) {
					return;
				};

				var typeDecl = fieldDecl.Parent as TypeDeclarationSyntax;
				var constructors = typeDecl.GetConstructors();

				if (!constructors.Any()) {
					context.RegisterRefactoring(CodeAction.Create(
						"Create new constructor",
						c => CreateConstructor(context.Document, fieldDecl, c)));
				}
				else {
					if (constructors.Count() == 1) {
						if (NeedInject(context, fieldDecl, constructors.First())) {
							context.RegisterRefactoring(CodeAction.Create(
								"Inject from constructor",
								c => InjectFromConstructor(context.Document, fieldDecl, constructors.First(), c)));
						}
					}
					else {
						var index = 1;
						foreach (var constructor in constructors) {
							if (NeedInject(context, fieldDecl, constructor)) {
								context.RegisterRefactoring(CodeAction.Create(
									string.Format("Inject from constructor #{0}", index),
									c => InjectFromConstructor(context.Document, fieldDecl, constructor, c)));

								index++;
							}
						}
					}
				}
			}
			catch {
				return;
			}
		}

		private static bool CanHandle(FieldDeclarationSyntax fieldDecl) {
			if (fieldDecl == null) {
				return false;
			}

			if (fieldDecl.Modifiers.Select(m => m.CSharpKind()).Contains(SyntaxKind.ConstKeyword)) {
				return false;
			}

			if (fieldDecl.Modifiers.Select(m => m.CSharpKind()).Contains(SyntaxKind.StaticKeyword)) {
				return false;
			}

			var variableDecl = fieldDecl.Declaration?.Variables.FirstOrDefault();
			if (variableDecl == null) {
				return false;
			}

			return true;
		}

		private bool NeedInject(CodeRefactoringContext context, FieldDeclarationSyntax fieldDecl, ConstructorDeclarationSyntax constructorDeclarationSyntax) {
			var varDecl = fieldDecl.Declaration.Variables.FirstOrDefault();
			var fieldName = varDecl.Identifier.Text;
			var assigments = constructorDeclarationSyntax.Body?.DescendantNodes().OfType<AssignmentExpressionSyntax>().ToList();

			foreach (var assigment in assigments) {
				if (context.CancellationToken.IsCancellationRequested) {
					return false;
				}

				if ((assigment.Left as IdentifierNameSyntax)?.Identifier.Text == fieldName) {
					return false;
				}

				var memberAccess = (assigment.Left as MemberAccessExpressionSyntax);
				if (memberAccess?.Expression is ThisExpressionSyntax && memberAccess?.Name?.Identifier.Text == fieldName) {
					return false;
				}
			}

			return true;
		}

		private async Task<Document> CreateConstructor(Document document, FieldDeclarationSyntax fieldDecl, CancellationToken c) {
			try {
				var varDecl = fieldDecl.Declaration.Variables.FirstOrDefault();
				var fieldName = varDecl.Identifier.Text;
				var typeDecl = fieldDecl.Parent as TypeDeclarationSyntax;

				var ctorDecl = SyntaxFactory
					.ConstructorDeclaration(typeDecl.Identifier)
					.WithBody(SyntaxFactory.Block())
					.WithLeadingTrivia(fieldDecl.GetLeadingTrivia())
					.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);

				var root = await document.GetSyntaxRootAsync(c);

				var newRoot = root.ReplaceNode(typeDecl, typeDecl.InsertNodesAfter(fieldDecl, new[] { ctorDecl }));

				typeDecl = newRoot.TypeWithName(typeDecl.Identifier.Text);
				fieldDecl = typeDecl.FieldWithName(fieldName);
				ctorDecl = typeDecl.GetConstructors().First();

				newRoot = AddParameterToConstructor(newRoot, fieldDecl, ctorDecl);

				return document.WithSyntaxRoot(newRoot);
			}
			catch {
				return document;
			}
		}

		private async Task<Document> InjectFromConstructor(Document document, FieldDeclarationSyntax fieldDecl, ConstructorDeclarationSyntax ctorDecl, CancellationToken c) {
			try {
				var root = await document.GetSyntaxRootAsync(c);
				root = AddParameterToConstructor(root, fieldDecl, ctorDecl);
				return document.WithSyntaxRoot(root);
			}
			catch {
				return document;
			}
		}

		private static SyntaxNode AddParameterToConstructor(SyntaxNode root, FieldDeclarationSyntax fieldDecl, ConstructorDeclarationSyntax ctorDecl) {
			var varDecl = fieldDecl.Declaration.Variables.FirstOrDefault();
			var fieldName = varDecl.Identifier.Text;
			var parameterName = fieldName.If(f => f.StartsWith("_"), f => f.Substring("_".Length));

			var newCtor = ctorDecl
				.AddParameterListParameters(
					SyntaxFactory.Parameter(
						SyntaxFactory.List<AttributeListSyntax>(),
						SyntaxFactory.TokenList(),
						fieldDecl.Declaration.Type,
						SyntaxFactory.Identifier(parameterName),
						null))
				.WithBody(ctorDecl.Body.AddStatements(
					SyntaxFactory.ExpressionStatement(
						SyntaxFactory.AssignmentExpression(
							SyntaxKind.SimpleAssignmentExpression,
							fieldName == parameterName
								? SyntaxFactory.IdentifierName(fieldName).OfThis()
								: (ExpressionSyntax)SyntaxFactory.IdentifierName(fieldName),
							SyntaxFactory.IdentifierName(parameterName)))))
				.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);

			root = root.ReplaceNode(ctorDecl, newCtor);

			return root;
		}
	}
}