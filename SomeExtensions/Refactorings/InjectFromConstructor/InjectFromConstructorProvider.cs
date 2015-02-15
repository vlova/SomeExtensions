using System.Collections.Generic;
using System.Composition;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
	// TODO: inject all
	[ExportCodeRefactoringProvider(nameof(InjectFromConstructorProvider), CSharp), Shared]
	public class InjectFromConstructorProvider : BaseRefactoringProvider<SyntaxNode> {
		protected override void ComputeRefactorings(RefactoringContext context, SyntaxNode node) {
			var injectParameter = Helpers.GetInjectParameter(node);
			if (injectParameter == null) {
				return;
			}

			var constructors = injectParameter?.DeclaredType?.FindConstructors();

			if (constructors.IsEmpty()) {
				context.RegisterAsync(new CreateConstructor(injectParameter));
			}
			else if (constructors.IsSingle()) {
				RegisterForOne(context, injectParameter, constructors.First());
			}
			else {
				RegisterForAll(context, injectParameter, constructors);
			}
		}

		private static void RegisterForOne(RefactoringContext context, InjectParameter parameter, ConstructorDeclarationSyntax ctor) {
			if (!Helpers.NeedInject(parameter, ctor, context.CancellationToken)) {
				return;
			}

			context.RegisterAsync(new InjectFromConstructor(parameter, ctor));
		}

		private static void RegisterForAll(RefactoringContext context, InjectParameter parameter, IEnumerable<ConstructorDeclarationSyntax> ctors) {
			context.RegisterAsync(new InjectFromAllConstructors(parameter));

			var index = 1;
			foreach (var ctor in ctors.WhileOk(context.CancellationToken)) {
				if (!Helpers.NeedInject(parameter, ctor, context.CancellationToken)) {
					continue;
				}

				context.RegisterAsync(new InjectFromConstructor(parameter, ctor, index));

				index++;
			}
		}
	}

}
