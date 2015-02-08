using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using System.Diagnostics.Contracts;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
	internal static class Helpers {
		public static bool NeedInject(InjectParameter parameter, ConstructorDeclarationSyntax constructor, CancellationToken token) {
			Contract.Requires(parameter != null);
			Contract.Requires(constructor != null);

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
			Contract.Requires(node != null);

			return GetFieldAsInjectParameter(node) ?? GetPropertyAsInjectParameter(node);
		}

		public static InjectParameter GetFieldAsInjectParameter(SyntaxNode node) {
			Contract.Requires(node != null);

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
			Contract.Requires(node != null);

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
			Contract.Requires(field != null);

			return !field.IsConstant()
				&& !field.IsStatic()
				&& field.HasOneVariable();
		}

		private static bool CanHandle(PropertyDeclarationSyntax property) {
			Contract.Requires(property != null);

			var getAccessor = property.GetAccessor();
			var setAccessor = property.SetAccessor();

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
