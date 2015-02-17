using System;
using System.Composition;
using System.Threading;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Refactorings.ToLinq.Transformers;
using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.ToLinq {
	[ExportCodeRefactoringProvider(nameof(ToLinqProvider), CSharp), Shared]
	internal class ToLinqProvider : BaseRefactoringProvider<ForEachStatementSyntax> {
		protected override int? FindUpLimit => 2;

		protected override bool IsGood(ForEachStatementSyntax @foreach)
			=> true;

		private static Func<ForEachStatementSyntax, ILinqTransformer>[] transformerFactories
			= new Func<ForEachStatementSyntax, ILinqTransformer>[] {
				//_ => new SelectTransformer(_),
				_ => new TakeWhileTransformer(_),
                _ => new WhereTransformer(_)
			};

		protected override void ComputeRefactorings(RefactoringContext context, ForEachStatementSyntax @foreach) {
			var refactoring = new ToLinqRefactoring(@foreach, transformerFactories);
			if (refactoring.CanTransform(context.Root)) {
				context.Register(refactoring);
			}
		}
	}

	internal interface ILinqTransformer {
		bool CanTransform(CompilationUnitSyntax root);

		Tuple<CompilationUnitSyntax, ForEachStatementSyntax> Transform(CompilationUnitSyntax root, CancellationToken token);
	}
}
