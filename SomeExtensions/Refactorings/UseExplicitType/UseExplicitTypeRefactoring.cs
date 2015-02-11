using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.UseExplicityType {
	internal class UseExplicitTypeRefactoring : IRefactoring {
		private readonly ITypeSymbol _type;
		private readonly LocalDeclarationStatementSyntax _local;

		public UseExplicitTypeRefactoring(LocalDeclarationStatementSyntax local, ITypeSymbol type) {
			Contract.Requires(local != null);
			Contract.Requires(type != null);

			_local = local;
			_type = type;
		}

		public string Description => "Use explicit type";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			return root.ReplaceNode(
				_local.Declaration,
				_local.Declaration.WithType(_type.ToTypeSyntax()).Nicefy());
		}
	}
}