﻿using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Refactorings;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Tests.Refactorings.ToAsync {
	[ExportCodeRefactoringProvider(nameof(ToAsyncProvider), LanguageNames.CSharp), Shared]
	internal class ToAsyncProvider : BaseRefactoringProvider<MethodDeclarationSyntax> {
		protected override bool IsGood(MethodDeclarationSyntax method)
			=> !method.Modifiers.Any(_ => _.IsKind(AsyncKeyword));

		protected override void ComputeRefactorings(RefactoringContext context, MethodDeclarationSyntax method) {
			// TODO: add analyzation of method usages;

			var newNode = method
				.WithReturnType(
					GenericName("Task".ToIdentifier(),
					TypeArgumentList(method.ReturnType.ItemToSeparatedList())))
				.AddModifiers(Token(AsyncKeyword).Formattify());

            context.Register("To async",
				root => root
					.ReplaceNode(method, newNode)
					.AddUsingIfNotExists("System.Threading.Tasks"));
        }
	}
}