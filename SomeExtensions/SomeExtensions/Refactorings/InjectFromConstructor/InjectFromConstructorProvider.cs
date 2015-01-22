using System.Collections.Generic;
using System.Composition;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Roslyn;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
	[ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
	internal class InjectFromConstructorProvider : BaseRefactoringProvider {
		public const string RefactoringId = "InjectFromConstructor";

		protected override void ComputeRefactorings(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
			var injectParameter = Helpers.GetInjectParameter(node);
			if (injectParameter == null) {
				return;
			}

			var constructors = injectParameter?.DeclaredType?.FindConstructors();

			if (constructors.IsEmpty()) {
				context.RegisterRefactoring(new CreateConstructor(injectParameter));
			}
			else if (constructors.IsSingle()) {
				RegisterForOne(context, injectParameter, constructors.First());
			}
			else {
				RegisterForAll(context, injectParameter, constructors);
			}
		}

		private static void RegisterForOne(CodeRefactoringContext context, InjectParameter parameter, ConstructorDeclarationSyntax ctor) {
			if (!Helpers.NeedInject(parameter, ctor, context.CancellationToken)) {
				return;
			}

			context.RegisterRefactoring(new InjectFromConstructor(parameter, ctor));
		}

		private static void RegisterForAll(CodeRefactoringContext context, InjectParameter parameter, IEnumerable<ConstructorDeclarationSyntax> ctors) {
			context.RegisterRefactoring(new InjectFromAllConstructors(parameter));

			var index = 1;
			foreach (var ctor in ctors.WhileOk(context.CancellationToken)) {
				if (!Helpers.NeedInject(parameter, ctor, context.CancellationToken)) {
					continue;
				}

				context.RegisterRefactoring(new InjectFromConstructor(parameter, ctor, index));

				index++;
			}
		}
	}

}
