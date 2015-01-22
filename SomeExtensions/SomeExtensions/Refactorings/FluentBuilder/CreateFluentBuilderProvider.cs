using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Roslyn;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.FluentBuilder {
	[ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
	internal class CreateFluentBuilderProvider : BaseRefactoringProvider {
		public const string RefactoringId = nameof(CreateFluentBuilderProvider);

		protected override void ComputeRefactorings(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
			var constructor = node.FindUp<ConstructorDeclarationSyntax>();

			if (constructor?.ParameterList?.Parameters.Count.Equals(0) ?? true) {
				return;
			}

			context.RegisterRefactoring(new CreateFluentBuilderRefactoring(
				context.Document,
				constructor.Parent.As<TypeDeclarationSyntax>(),
				constructor));
		}
	}
}
