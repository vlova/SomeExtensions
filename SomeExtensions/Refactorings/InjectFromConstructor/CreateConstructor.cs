﻿using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions.Syntax;
using System.Diagnostics.Contracts;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
	internal class CreateConstructor : IRefactoring {
		private readonly InjectParameter _parameter;

		public CreateConstructor(InjectParameter parameter) {
			Contract.Requires(parameter != null);
			_parameter = parameter;
		}

		public string Description {
			get {
				return "Create new public constructor";
			}
		}

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			var type = _parameter.DeclaredType as TypeDeclarationSyntax;

			var ctor = SyntaxFactory
				.ConstructorDeclaration(type.Identifier)
				.WithModifiers(SyntaxKind.PublicKeyword)
				.WithBody(SyntaxFactory.Block())
				.Nicefy();

			var newRoot = root.ReplaceNode(type, type.InsertAfter(_parameter.Node, ctor));
			var generatedCtor = newRoot.Find().Type(type).FindConstructors().First();

			return new InjectFromConstructor(_parameter, generatedCtor)
				.ComputeRoot(newRoot, token);
		}
	}
}