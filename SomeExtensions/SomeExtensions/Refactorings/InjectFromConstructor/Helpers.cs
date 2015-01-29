using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
	internal static class Helpers {
		public static bool NeedInject(InjectParameter parameter, ConstructorDeclarationSyntax constructor, CancellationToken token) {
			var fieldName = parameter.Name;
			var assigments = constructor.Body?.DescendantNodes().OfType<AssignmentExpressionSyntax>().ToList();

			foreach (var assigment in assigments.WhileOk(token)) {
				if (assigment.GetAssigneeName(allowThisAccess: true) == fieldName) {
					return false;
				}
			}

			return !token.IsCancellationRequested;
		}

		public static InjectParameter GetInjectParameter(SyntaxNode node) {
			return GetFieldAsInjectParameter(node) ?? GetPropertyAsInjectParameter(node);
		}

		public static InjectParameter GetFieldAsInjectParameter(SyntaxNode node) {
			var field = node.FindUp<FieldDeclarationSyntax>();
			if (field == null || !CanHandle(field)) {
				return null;
			}

			var name = field.Declaration.Variables.FirstOrDefault().Identifier.Text;
			var parameterType = field.Declaration.Type;
            var declaredType = field.Parent as TypeDeclarationSyntax;

			return new InjectParameter(field, name, parameterType, declaredType);
		}

		public static InjectParameter GetPropertyAsInjectParameter(SyntaxNode node) {
			var property = node.FindUp<PropertyDeclarationSyntax>();
			if (property == null || !CanHandle(property)) {
				return null;
			}

			return new InjectParameter(
				property,
				property.Identifier.Text,
				property.Type,
				property.Parent.As<TypeDeclarationSyntax>());
		}

		private static bool CanHandle(FieldDeclarationSyntax field) {
			return !field.IsConstant()
				&& !field.IsStatic()
				&& field.HasOneVariable();
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
