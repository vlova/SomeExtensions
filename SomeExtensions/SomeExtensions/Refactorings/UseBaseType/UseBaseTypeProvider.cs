using System.Collections;
using System.Composition;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using SomeExtensions.Extensions.Roslyn;

namespace SomeExtensions.Refactorings.UseBaseType {
	[ExportCodeRefactoringProvider(RefactoringId, LanguageNames.CSharp), Shared]
	public class UseBaseTypeProvider : BaseRefactoringProvider {
		public const string RefactoringId = nameof(UseBaseTypeProvider);

		private static readonly string[] _badSystemTypes = new[] {
			"IComparable",
			"ICloneable",
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
			SpecialType.System_ValueType
		};

		protected override async Task ComputeRefactoringsAsync(CodeRefactoringContext context, SyntaxNode root, SyntaxNode node) {
			ExpressionSyntax typeNode = node.FindUp<TypeSyntax>();
			if (typeNode == null) {
				return;
			}

			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);

			var typeSymbol = GetTypeSymbol(typeNode, semanticModel);

			// this is the ugly hack
			// dunno why, but studio crashes on base types of System.Type
			if (typeSymbol.ToDisplayString() == "System.Type") {
				return;
			}

			if (IsGoodType(typeSymbol.BaseType, semanticModel)) {
				context.RegisterRefactoring(root, new UseBaseTypeRefactoring(typeNode, typeSymbol));
			}

			foreach (var interfaceType in typeSymbol.AllInterfaces) {
				if (IsGoodType(interfaceType, semanticModel)) {
					context.RegisterRefactoring(root, new UseBaseTypeRefactoring(typeNode, interfaceType));
				}
			}
		}

		private static ITypeSymbol GetTypeSymbol(ExpressionSyntax node, SemanticModel semanticModel) {
			ITypeSymbol typeSymbol;

			if (node.As<IdentifierNameSyntax>()?.Identifier.Text == "var") {
				node = node.Parent.As<VariableDeclarationSyntax>()
					?.Variables.FirstOrDefault()
					?.Initializer?.Value;

				typeSymbol = semanticModel.GetExpressionType(node);
			}
			else {
				typeSymbol = semanticModel.GetTypeSymbol(node);
			}

			return typeSymbol;
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

			if (type.Name.In(_badSystemTypes)) {
				return false;
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
