using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Roslyn;

using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.FluentBuilder {
	[ExportCodeRefactoringProvider(nameof(CreateFluentBuilderProvider), CSharp), Shared]
	internal class CreateFluentBuilderProvider : BaseRefactoringProvider<ConstructorDeclarationSyntax> {
		protected override int? FindUpLimit => 2;

		protected override void ComputeRefactorings(CodeRefactoringContext context, SyntaxNode root, ConstructorDeclarationSyntax constructor) {
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
