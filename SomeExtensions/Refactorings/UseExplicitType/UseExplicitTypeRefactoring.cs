﻿using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;
using static System.Diagnostics.Contracts.Contract;

namespace SomeExtensions.Refactorings.UseExplicitType {
	internal class UseExplicitTypeRefactoring : IRefactoring {
		private readonly ITypeSymbol _type;
		private readonly LocalDeclarationStatementSyntax _local;

		public UseExplicitTypeRefactoring(LocalDeclarationStatementSyntax local, ITypeSymbol type) {
			Requires(local != null);
			Requires(type != null);

			_local = local;
			_type = type;
		}

		public string Description => "Use explicit type";

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root) {
			return root.ReplaceNode(
				_local.Declaration,
				_local.Declaration.WithType(_type.ToTypeSyntax()).Nicefy());
		}
	}
}