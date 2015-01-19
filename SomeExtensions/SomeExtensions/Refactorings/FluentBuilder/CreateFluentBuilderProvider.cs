using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using System.Composition;

namespace SomeExtensions.Refactorings.FluentBuilder {
	[ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
	internal class CreateFluentBuilderProvider : BaseRefactoringProvider {
		public const string RefactoringId = nameof(CreateFluentBuilderProvider);

		protected override void ComputeRefactorings(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
			var constructor = node.FindUp<ConstructorDeclarationSyntax>();

			if (constructor.Modifiers.Any() && constructor.HasModifier(SyntaxKind.PrivateKeyword)) {
				return;
			}

			var parameters = constructor?.ParameterList?.Parameters;
			if (parameters?.Count == 0) {
				return;
			}

			context.RegisterRefactoring(new CreateFluentBuilderRefactoring(
				context.Document,
				constructor.Parent.As<TypeDeclarationSyntax>(),
				constructor));
		}
	}
}
