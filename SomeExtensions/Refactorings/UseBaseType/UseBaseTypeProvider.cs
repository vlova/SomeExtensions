using System.Collections;
using System.Composition;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using static Microsoft.CodeAnalysis.Accessibility;

namespace SomeExtensions.Refactorings.UseBaseType {
	[ExportCodeRefactoringProvider(nameof(UseBaseTypeProvider), LanguageNames.CSharp), Shared]
	internal class UseBaseTypeProvider : BaseRefactoringProvider<TypeSyntax> {
		protected override int? FindUpLimit => 2;

		protected override async Task ComputeRefactoringsAsync(RefactoringContext context, TypeSyntax typeNode) {
			var semanticModel = await context.SemanticModelAsync;
			var typeSymbol = Helpers.GetTypeSymbol(typeNode, semanticModel);

			// this is the ugly hack
			// dunno why, but studio crashes on base types of System.Type
			if (typeSymbol.ToDisplayString() == "System.Type") {
				return;
			}

			if (IsGoodType(typeSymbol.BaseType, semanticModel)) {
				context.Register(new UseBaseTypeRefactoring(typeNode, typeSymbol.BaseType));
			}

			foreach (var interfaceType in typeSymbol.AllInterfaces) {
				if (IsGoodType(interfaceType, semanticModel)) {
					context.Register(new UseBaseTypeRefactoring(typeNode, interfaceType));
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
				if (type.DeclaredAccessibility != Public) {
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
