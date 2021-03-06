﻿using System.Composition;

using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.ToReadonlyProperty {
	[ExportCodeRefactoringProvider(nameof(ToReadonlyPropertyProvider), CSharp), Shared]
	public class ToReadonlyPropertyProvider : BaseRefactoringProvider<PropertyDeclarationSyntax> {
		protected override int? FindUpLimit => 3;

		protected override bool IsGood(PropertyDeclarationSyntax property) => property.IsAutomaticProperty() && !property.IsReadonlyProperty();

		protected override void ComputeRefactorings(RefactoringContext context, PropertyDeclarationSyntax property) {
			if (!property.IsAutomaticProperty() || property.IsReadonlyProperty()) {
				return;
			}

			context.RegisterAsync(new ToReadonlyPropertyRefactoring(property));
		}
	}
}
