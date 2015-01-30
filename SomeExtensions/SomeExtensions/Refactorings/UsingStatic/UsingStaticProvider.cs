using System.Composition;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Roslyn;

using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Microsoft.CodeAnalysis.LanguageNames;
using static Microsoft.CodeAnalysis.SymbolKind;

namespace SomeExtensions.Refactorings.UsingStatic {
	[ExportCodeRefactoringProvider(nameof(UsingStaticProvider), CSharp), Shared]
	internal class UsingStaticProvider : BaseRefactoringProvider<MemberAccessExpressionSyntax> {
		protected override int? FindUpLimit => 2;

		protected async override Task ComputeRefactoringsAsync(CodeRefactoringContext context, SyntaxNode root, MemberAccessExpressionSyntax memberAccess) {
			if (memberAccess?.CSharpKind() != SimpleMemberAccessExpression) {
				return;
			}

			var model = await context.Document.GetSemanticModelAsync(context.CancellationToken);
			var symbolInfo = model.GetSymbolInfo(memberAccess.Expression);

			if (!(symbolInfo.Symbol?.CanBeReferencedByName ?? false)) {
				return;
			}

			if (symbolInfo.Symbol?.Kind == NamedType) {
				context.RegisterRefactoring(root, new UsingStaticRefactoring(
					memberAccess,
					symbolInfo.Symbol as ITypeSymbol));
			}
		}
	}
}
