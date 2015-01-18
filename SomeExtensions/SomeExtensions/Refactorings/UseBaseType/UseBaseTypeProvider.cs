using System.Collections;
using System.Composition;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.UseBaseType {
	[ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
	public class UseBaseTypeProvider : BaseRefactoringProvider {
		public const string RefactoringId = nameof(UseBaseTypeProvider);

		private static readonly string[] _badSystemTypes = new[] {
			"IComparable",
			"IEquatable",
			"IConvertible",
			"IFormattable",
			"ISerializable"
		};

		private static readonly SpecialType[] _badSpecialTypes = new[] {
			SpecialType.System_Object,
			SpecialType.System_Enum,
			SpecialType.System_Delegate,
			SpecialType.System_MulticastDelegate,
			SpecialType.System_ValueType,
			// non-generic collections sucks
			SpecialType.System_Collections_IEnumerable,
			SpecialType.System_Collections_IEnumerator
		};

		protected override async Task ComputeRefactoringsAsync(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
			var typeNode = node.FindUp<TypeSyntax>();
			if (typeNode == null) {
				return;
			}

			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);

			var typeSymbol = semanticModel.GetSpeculativeTypeSymbol(typeNode);
			if (IsGoodType(typeSymbol.BaseType, semanticModel)) {
				context.RegisterRefactoring(root, new UseBaseTypeRefactoring(typeNode, typeSymbol));
			}

			foreach (var interfaceType in typeSymbol.Interfaces) {
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

			if (type.ContainingNamespace.ToDisplayString() == nameof(System)) {
                if (type.Name.In(_badSystemTypes)) {
					return false;
				}
			}

			if (type.ContainingNamespace.ToDisplayString() == typeof(IEnumerable).Namespace) {
				return false;
			}

			if (type.SpecialType.In(_badSpecialTypes)) {
				return false;
			}

			return true;
		}
	}
}
