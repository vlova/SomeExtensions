using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Refactorings.Contracts;
using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.FluentBuilder {
	[ExportCodeRefactoringProvider(nameof(FluentBuilderProvider), CSharp), Shared]
	[ExtensionOrder(After = nameof(RequiresParameterProvider))]
	internal class FluentBuilderProvider : BaseRefactoringProvider<ConstructorDeclarationSyntax> {
		protected override int? FindUpLimit => 2;

		protected override void ComputeRefactorings(RefactoringContext context, ConstructorDeclarationSyntax constructor) {
			if (constructor?.ParameterList?.Parameters.Count.Equals(0) ?? true) {
				return;
			}

			context.RegisterAsync(new FluentBuilderRefactoring(
				constructor.Parent.As<TypeDeclarationSyntax>(),
				constructor));
		}
	}
}
