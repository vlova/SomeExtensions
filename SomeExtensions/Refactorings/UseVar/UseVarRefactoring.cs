using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.UseVar {
	internal class UseVarRefactoring : IRefactoring {
		private readonly LocalDeclarationStatementSyntax _local;

		public UseVarRefactoring(LocalDeclarationStatementSyntax local) {
			Contract.Requires(local != null);
			_local = local;
		}

		public string Description => "Use var";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			return root.ReplaceNode(
				_local.Declaration,
				_local.Declaration.WithType("var".ToIdentifierName()));
		}
	}
}