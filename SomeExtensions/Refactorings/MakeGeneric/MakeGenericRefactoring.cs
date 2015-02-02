using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static SomeExtensions.Extensions.Syntax.SyntaxFactoryExtensions;

namespace SomeExtensions.Refactorings.MakeGeneric {
	internal class MakeGenericRefactoring : IRefactoring {
		private readonly MethodDeclarationSyntax _method;
		private readonly TypeSyntax _type;
		private readonly bool _inherit;

		public MakeGenericRefactoring(TypeSyntax type, MethodDeclarationSyntax method, bool inherit) {
			_type = type;
			_method = method;
			_inherit = inherit;
		}

		public string Description
			=> "Make method generic".If(_inherit, s => s + " using base constraint");

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
			var typesToReplace = _method.ParameterList.Parameters
				.Select(n => n.Type)
				.Append(_method.ReturnType)
				.SelectMany(t => t.DescendantNodes().OfType<TypeSyntax>().Prepend(t))
				.Where(type => type.GetPartialTypeName() == _type.GetPartialTypeName());

			var newMethod = _method
				.ReplaceNodes(typesToReplace, (o, r) => GetGenericTypeName().ToIdentifierName().Nicefy())
				.AddTypeParameterListParameters(GetTypeParameter());

			if (_inherit) {
				newMethod = newMethod.AddConstraintClauses(TypeParameterConstraint(GetGenericTypeName(), _type));
			}

			return root.ReplaceNode(_method, newMethod);
		}

		private string GetGenericTypeName() {
			return "T" + _type.GetText().ToString().Trim().UppercaseFirst();
		}

		private TypeParameterSyntax GetTypeParameter() {
			return TypeParameter(GetGenericTypeName())
                .WithUserRename()
				.Nicefy();
		}
	}
}