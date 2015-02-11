using System.Composition;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.UseVar {
	[ExportCodeRefactoringProvider(nameof(UseVarProvider), CSharp), Shared]
	internal class UseVarProvider : BaseRefactoringProvider<LocalDeclarationStatementSyntax> {
		protected override int? FindUpLimit => 3;

		protected override void ComputeRefactorings(RefactoringContext context, LocalDeclarationStatementSyntax local) {
			var declaration = local.Declaration;
            if (declaration?.Variables.Count != 1) return;
			if (declaration.Type.IsEquivalentTo("var".ToIdentifierName(), false)) return;

			var variable = declaration.Variables.First();
			if (variable.Initializer == null) return;

			context.Register(new UseVarRefactoring(local));
		}
	}
}
