﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Semantic;
using SomeExtensions.Extensions.Syntax;
using static System.Diagnostics.Contracts.Contract;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.UsingStatic {
	internal class UsingStaticRefactoring : IRefactoring {
		private readonly ITypeSymbol _symbol;
		private readonly MemberAccessExpressionSyntax _memberAccess;
		private readonly bool _fixAll;

		public UsingStaticRefactoring(MemberAccessExpressionSyntax memberAccess, ITypeSymbol symbol, bool fixAll) {
			Requires(memberAccess != null);
			Requires(symbol != null);

			_memberAccess = memberAccess;
			_symbol = symbol;
			_fixAll = fixAll;
		}

		public string Description => "Add using static directive".If(_fixAll, s => s + " (fix all)");

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root) {
			var nodes = GetNodes(root);

			return root
				.ReplaceNodes(nodes, (_, memberAccess) => memberAccess.Name.Nicefy())
				.ReplaceNode(_memberAccess, _memberAccess.Name.Nicefy())
				.AddUsingIfNotExists(_symbol.GetFullName(), @static: true);
		}

		private IEnumerable<MemberAccessExpressionSyntax> GetNodes(SyntaxNode root) {
			if (!_fixAll) {
				return new[] { _memberAccess };
			}

			return root
				.DescendantNodes<MemberAccessExpressionSyntax>()
				.Where(r => AreEquivalent(r.Expression, _memberAccess.Expression));
		}
	}
}