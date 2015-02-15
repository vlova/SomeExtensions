using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using static System.Diagnostics.Contracts.Contract;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace SomeExtensions.Refactorings.ToReadonlyField {
	internal class ToReadonlyFieldRefactoring : IRefactoring {
		private readonly bool _all;
		private readonly FieldDeclarationSyntax _field;

		public ToReadonlyFieldRefactoring(FieldDeclarationSyntax field, bool all) {
			Requires(field != null);
			_field = field;
			_all = all;
		}

		public string Description => "To readonly field".If(_all, s => s + " (all)");

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var fields = new[] { _field };

			if (_all) {
				fields = _field
					.Parent
					.ChildNodes()
					.OfType<FieldDeclarationSyntax>()
					.Where(f => !f.HasModifier(ReadOnlyKeyword))
					.ToArray();
			}

			return root.ReplaceNodes(fields, (field, _) => field.AppendModifiers(ReadOnlyKeyword));
		}
	}
}