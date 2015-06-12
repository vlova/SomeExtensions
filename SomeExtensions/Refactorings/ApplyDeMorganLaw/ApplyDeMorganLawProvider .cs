using System.Composition;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.ApplyDeMorganLaw {
	[ExportCodeRefactoringProvider(nameof(ApplyDeMorganLawProvider), CSharp), Shared]
	internal class ApplyDeMorganLawProvider : BaseRefactoringProvider<BinaryExpressionSyntax> {
		protected override int? FindUpLimit => 6;

		protected override bool IsGood(BinaryExpressionSyntax node)
			=> Helpers.CanConvert(node.Kind());

		protected override void ComputeRefactorings(RefactoringContext context, BinaryExpressionSyntax operation) {
			context.Register(new ApplyDeMorganLawRefactoring(operation));
		}
	}
}
