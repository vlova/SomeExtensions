using System.Diagnostics.Contracts;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions.Syntax {
	public static class FieldExtensions {
        public static bool HasOneVariable(this FieldDeclarationSyntax field) {
            return field?.Declaration?.Variables.Count == 1;
		}

		public static bool HasOneVariable(this VariableDeclarationSyntax variable) {
			return variable?.Variables.Count == 1;
		}

		public static string GetVariableName(this VariableDeclarationSyntax variables) {
			Contract.Requires(variables.HasOneVariable());

			return variables.Variables.First().Identifier.Text;
		}
	}
}
