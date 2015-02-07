﻿using System.Composition;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Microsoft.CodeAnalysis.LanguageNames;
using static Microsoft.CodeAnalysis.SymbolKind;

namespace SomeExtensions.Refactorings.UsingStatic {
	[ExportCodeRefactoringProvider(nameof(UsingStaticProvider), CSharp), Shared]
	internal class UsingStaticProvider : BaseRefactoringProvider<MemberAccessExpressionSyntax> {
		protected override int? FindUpLimit => 2;

		protected async override Task ComputeRefactoringsAsync(RefactoringContext context,  MemberAccessExpressionSyntax memberAccess) {
			if (memberAccess?.CSharpKind() != SimpleMemberAccessExpression) {
				return;
			}

			var model = await context.GetSemanticModelAsync();
			var symbolInfo = model.GetSymbolInfo(memberAccess.Expression);

			if (!(symbolInfo.Symbol?.CanBeReferencedByName ?? false)) {
				return;
			}

			if (symbolInfo.Symbol?.Kind == NamedType) {
				context.RegisterAsync(new UsingStaticRefactoring(
					memberAccess,
					symbolInfo.Symbol as ITypeSymbol,
					fixAll: false));

				var similar = context.Root
					.DescendantNodes<MemberAccessExpressionSyntax>()
					.Where(r => AreEquivalent(r.Expression, memberAccess.Expression));

                if (similar.HasAtLeast(2)) {
					context.RegisterAsync(new UsingStaticRefactoring(
						memberAccess,
						symbolInfo.Symbol as ITypeSymbol,
						fixAll: true));
				}
			}
		}
	}
}
