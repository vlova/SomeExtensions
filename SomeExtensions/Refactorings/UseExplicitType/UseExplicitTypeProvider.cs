using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Semantic;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.LanguageNames;

namespace SomeExtensions.Refactorings.UseExplicitType {
	[ExportCodeRefactoringProvider(nameof(UseExplicitTypeProvider), CSharp), Shared]
	internal class UseExplicitTypeProvider : BaseRefactoringProvider<LocalDeclarationStatementSyntax> {
		protected override int? FindUpLimit => 3;

		protected override async Task ComputeRefactoringsAsync(RefactoringContext context, LocalDeclarationStatementSyntax local) {
			var declaration = local.Declaration;
            if (declaration?.Variables.Count != 1) return;
			if (!declaration.Type.IsEquivalentTo("var".ToIdentifierName(), false)) return;

			var variable = declaration.Variables.First();
			if (variable.Initializer == null) return;

			var model = await context.SemanticModelAsync;

			var type = model.GetSpeculativeExpressionType(variable.Initializer.Value);
			if (type == null) return;
			if (!type.CanBeReferencedByName) return;

			context.Register(new UseExplicitTypeRefactoring(local, type));
		}
	}
}
