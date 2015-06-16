using System.Composition;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Refactorings.ToLinq.Aggregators;
using SomeExtensions.Refactorings.ToLinq.Simplifiers;
using SomeExtensions.Refactorings.ToLinq.Transformers;
using SomeExtensions.Transformers;
using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.ToLinq {
	[ExportCodeRefactoringProvider(nameof(ToLinqProvider), CSharp), Shared]
	internal class ToLinqProvider : BaseRefactoringProvider<ForEachStatementSyntax> {
		protected override int? FindUpLimit => 2;

		protected override bool IsGood(ForEachStatementSyntax @foreach)
			=> true;

		private static TransformerFactory<ForEachStatementSyntax> transformerFactories
			= Transformation.Composite<ForEachStatementSyntax>(
				_ => new TakeWhileTransformer(_),
				_ => new IfContinueToWhereTransformer(_),
				_ => new IfNoElseToWhereTransformer(_),
				_ => new SelectTransformer(_)
			);

		private static TransformerFactory<ForEachStatementSyntax, LocalDeclarationStatementSyntax> aggregateTransformers
			= Transformation.Composite<ForEachStatementSyntax, LocalDeclarationStatementSyntax>(
				_ => new ToListTransformer(_),
				_ => new AggregateTransformer(_)
			);

		private static TransformerFactory<InvocationExpressionSyntax> simplifierFactories
			= Transformation.Composite<InvocationExpressionSyntax>(
				_ => new SelectParenthesizedSimplifier(_),
				_ => new SelectIdentitySimplifier(_),
				_ => new AsCastNotNullToOfTypeSimplifier(_),
				_ => new WhereIsCastToOfTypeSimplifier(_),
				_ => new IntersectSimplifier(_),
				_ => new ExceptSimplifier(_),
				_ => new SumSimplifier(_),
				_ => new CastSimplifier(_)
			);


		protected override void ComputeRefactorings(RefactoringContext context, ForEachStatementSyntax @foreach) {
			var refactoring = new ToLinqRefactoring(@foreach,
				transformerFactories, aggregateTransformers, simplifierFactories);

			if (refactoring.CanTransform(context.Root)) {
				context.Register(refactoring);
			}
		}
	}
}
