using System.Diagnostics.Contracts;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions.Syntax {
	public static class FieldExtensions {
		public static bool HasOneVariable(this BaseFieldDeclarationSyntax field)
			=> field?.Declaration?.Variables.Count == 1;

		public static bool HasOneVariable(this VariableDeclarationSyntax variable)
			=> variable?.Variables.Count == 1;

		public static string GetVariableName(this VariableDeclarationSyntax variables)
			=> variables.GetVariable().Identifier.Text;

		public static VariableDeclaratorSyntax GetVariable(this LocalDeclarationStatementSyntax local)
			=> local.Declaration.GetVariable();

		public static VariableDeclaratorSyntax GetVariable(this VariableDeclarationSyntax variables) {
			Contract.Requires(variables.HasOneVariable());

			return variables.Variables.First();
		}
	}
}
