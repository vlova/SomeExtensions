using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions.Syntax {
	public static class AssigmentExtensions {
		public static string GetAssigneeName(this AssignmentExpressionSyntax assigment, bool allowThisAccess = false) {
			return GetAssigneeSimpleName(assigment)
				?? (allowThisAccess ? GetAssigneeThisName(assigment) : null);
		}

		private static string GetAssigneeSimpleName(AssignmentExpressionSyntax assigment) {
			return assigment?.Left.As<IdentifierNameSyntax>()?.Identifier.Text;
		}

		private static string GetAssigneeThisName(AssignmentExpressionSyntax assigment) {
			var memberAccess = assigment.Left as MemberAccessExpressionSyntax;
			if (memberAccess?.Expression is ThisExpressionSyntax) {
				return memberAccess?.Name?.Identifier.Text;
			}

			return null;
		}
	}
}
