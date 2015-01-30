using System.Collections;
using System.Composition;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Roslyn;

namespace SomeExtensions.Refactorings.UseBaseType {
	[ExportCodeRefactoringProvider(nameof(UseBaseTypeProvider), LanguageNames.CSharp), Shared]
	internal class UseBaseTypeProvider : BaseRefactoringProvider<TypeSyntax> {
		protected override int? FindUpLimit => 2;

		protected override async Task ComputeRefactoringsAsync(CodeRefactoringContext context, SyntaxNode root, TypeSyntax typeNode) {
			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
			var typeSymbol = Helpers.GetTypeSymbol(typeNode, semanticModel);

			// this is the ugly hack
			// dunno why, but studio crashes on base types of System.Type
			if (typeSymbol.ToDisplayString() == "System.Type") {
				return;
			}

			if (IsGoodType(typeSymbol.BaseType, semanticModel)) {
				context.RegisterRefactoring(root, new UseBaseTypeRefactoring(typeNode, typeSymbol.BaseType));
			}

			foreach (var interfaceType in typeSymbol.AllInterfaces) {
				if (IsGoodType(interfaceType, semanticModel)) {
					context.RegisterRefactoring(root, new UseBaseTypeRefactoring(typeNode, interfaceType));
				}
			}
		}

		private bool IsGoodType(INamedTypeSymbol type, SemanticModel semanticModel) {
			if (type == null) {
				return false;
			}

			if (!type.CanBeReferencedByName) {
				return false;
			}

			if (type.ContainingAssembly != semanticModel.Compilation.Assembly) {
				if (type.DeclaredAccessibility != Accessibility.Public) {
					return false;
				}
			}

			if (type.Name.In(Helpers.BadSystemTypes)) {
				return false;
			}

			if (type.ContainingNamespace.ToDisplayString() == typeof(IEnumerable).Namespace) {
				return false;
			}

			if (type.SpecialType.In(Helpers.BadSpecialTypes)) {
				return false;
			}

			return true;
		}
	}
}
