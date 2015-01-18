using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
	internal static class Helpers {
		public static bool NeedInject(InjectParameter parameter, ConstructorDeclarationSyntax constructor, CancellationToken token) {
			var fieldName = parameter.Name;
			var assigments = constructor.Body?.DescendantNodes().OfType<AssignmentExpressionSyntax>().ToList();

			foreach (var assigment in assigments) {
				if (token.IsCancellationRequested) {
					return false;
				}

				if ((assigment.Left as IdentifierNameSyntax)?.Identifier.Text == fieldName) {
					return false;
				}

				var memberAccess = (assigment.Left as MemberAccessExpressionSyntax);
				if (memberAccess?.Expression is ThisExpressionSyntax && memberAccess?.Name?.Identifier.Text == fieldName) {
					return false;
				}
			}

			return true;
		}

		public static InjectParameter GetFieldAsInjectParameter(SyntaxNode node) {
			var field = node.FindUp<FieldDeclarationSyntax>();
			if (field != null) {
				if (!CanHandle(field)) {
					return null;
				};

				var type = field.Parent as TypeDeclarationSyntax;
				var name = field.Declaration.Variables.FirstOrDefault().Identifier.Text;

				return new InjectParameter(
					field,
					name,
					field.Declaration.Type,
					field.Parent.As<TypeDeclarationSyntax>());
			}

			return null;
		}

		public static InjectParameter GetPropertyAsInjectParameter(SyntaxNode node) {
			var property = node.FindUp<PropertyDeclarationSyntax>();
			if (property != null) {
				return new InjectParameter(
					property,
					property.Identifier.Text,
					property.Type,
					property.Parent.As<TypeDeclarationSyntax>());
			}

			return null;
		}

		public static InjectParameter GetInjectParameter(SyntaxNode node) {
			return GetFieldAsInjectParameter(node) ?? GetPropertyAsInjectParameter(node);
		}

		private static bool CanHandle(FieldDeclarationSyntax field) {
			return !field.IsConstant()
				&& !field.IsStatic()
				&& field.ContainsOneVariable();
		}

		private static bool CanHandle(PropertyDeclarationSyntax property) {
			var getAccessor = property.AccessorList.GetAccessor();
			var setAccessor = property.AccessorList.SetAccessor();

			var isFullAuto = (getAccessor != null
				&& getAccessor.Body != null
				&& setAccessor != null
				&& setAccessor.Body != null);

			var isAutoSet = (getAccessor != null
				&& getAccessor.Body == null
				&& setAccessor == null);

			return isFullAuto || isAutoSet;
		}
	}
}
