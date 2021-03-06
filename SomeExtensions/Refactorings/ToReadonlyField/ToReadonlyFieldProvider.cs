﻿using System.Composition;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.ToReadonlyField {
	[ExportCodeRefactoringProvider(nameof(ToReadonlyFieldProvider), CSharp), Shared]
	internal sealed class ToReadonlyFieldProvider : BaseRefactoringProvider<FieldDeclarationSyntax> {
		protected override bool IsGood(FieldDeclarationSyntax field) => !field.HasModifier(ReadOnlyKeyword);

		protected override void ComputeRefactorings(RefactoringContext context, FieldDeclarationSyntax field) {
			context.Register(new ToReadonlyFieldRefactoring(field, all: false));
			context.RegisterAsync(new ToReadonlyFieldRefactoring(field, all: true));
		}
	}
}
