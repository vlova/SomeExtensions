using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions.Syntax {
	public static class FieldExtensions {
        public static bool ContainsOneVariable(this FieldDeclarationSyntax field) {
            return field?.Declaration?.Variables.Count == 1;
        }
    }
}
