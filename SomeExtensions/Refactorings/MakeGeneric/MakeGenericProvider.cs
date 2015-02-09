using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Semantic;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Refactorings.MakeGeneric {
	[ExportCodeRefactoringProvider(nameof(MakeGenericProvider), LanguageNames.CSharp), Shared]
	internal class MakeGenericProvider : BaseRefactoringProvider<SyntaxNode> {
		private static readonly SyntaxKind[] _sealedTypes = new[] {
			ByteKeyword,
			BoolKeyword,
			SByteKeyword,
			ShortKeyword,
			UShortKeyword,
			IntKeyword,
			UIntKeyword,
			LongKeyword,
			ULongKeyword,
			DoubleKeyword,
			FloatKeyword,
			DecimalKeyword,
			StringKeyword,
			CharKeyword,
			VoidKeyword,
		};

		protected override int? FindUpLimit => 2;

		protected override async Task ComputeRefactoringsAsync(RefactoringContext context, SyntaxNode node) {
			var type = node.FindUp<TypeSyntax>() ?? node.FindUp<ParameterSyntax>()?.Type;
			if (type == null) return;
			if (type is GenericNameSyntax) return; // TIenumerable<object> obviously doesn't makes a sense

			var method = type?.Fluent(t => FindMethod(t));
			if (method == null) return;
			if (method.HasModifier(OverrideKeyword)) return; // how you can change inherited method?
			if (type.IsGenericTypeParameterOf(method)) return;
			if (type.ContainsGenericTypeParameterOf(method)) return;

			context.Register(new MakeGenericRefactoring(type, method, inherit: false));
			if (await CanInherit(context, type)) {
				context.Register(new MakeGenericRefactoring(type, method, inherit: true));
			}
		}

		protected MethodDeclarationSyntax FindMethod(TypeSyntax type) {
			type = type.GetThisAndParents()
				.TakeWhile(t => t is TypeSyntax)
				.Cast<TypeSyntax>()
				.LastOrDefault();

			return type.Parent.As<MethodDeclarationSyntax>()
				?? type.FindUp<ParameterSyntax>()?.Parent?.Parent.As<MethodDeclarationSyntax>();
		}

		public static async Task<bool> CanInherit(RefactoringContext context, TypeSyntax type) {
			var predefinedType = type as PredefinedTypeSyntax;

			if (predefinedType?.Keyword.CSharpKind().In(_sealedTypes) ?? false) {
				return false;
			}

			if (predefinedType?.Keyword.IsKind(ObjectKeyword) ?? false) {
				return false;
			}

			var model = await context.SemanticModelAsync;

			var typeSymbol = model.GetTypeSymbol(type);
			if (typeSymbol.IsValueType || typeSymbol.IsSealed) {
				return false;
			}

			return true;
		}
	}
}
