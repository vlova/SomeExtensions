using System.Composition;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
	// TODO: inject properties too
	[ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
	internal class InjectFromConstructorProvider : BaseRefactoringProvider {
		public const string RefactoringId = "InjectFromConstructor";

		protected override void ComputeRefactorings(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
			var injectParameter = Helpers.GetInjectParameter(node);
			if (injectParameter == null) {
				return;
			}

			var constructors = injectParameter.DeclaredType.FindConstructors();

			if (!constructors.Any()) {
				context.RegisterRefactoring(new CreateConstructor(injectParameter));
				return;
			}

			if (constructors.Count() == 1) {
				if (Helpers.NeedInject(injectParameter, constructors.First(), context.CancellationToken)) {
					context.RegisterRefactoring(new InjectFromConstructor(
						injectParameter,
						constructors.First()));
				}
			}
			else {
				context.RegisterRefactoring(new InjectFromAllConstructors(injectParameter));

				var index = 1;
				foreach (var constructor in constructors) {
					if (Helpers.NeedInject(injectParameter, constructor, context.CancellationToken)) {
						context.RegisterRefactoring(new InjectFromConstructor(
							injectParameter,
							constructor,
							index));

						index++;
					}
				}
			}
		}
	}

}
