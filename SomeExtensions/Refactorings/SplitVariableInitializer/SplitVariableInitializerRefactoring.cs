﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Semantic;
using SomeExtensions.Extensions.Syntax;
using static System.Diagnostics.Contracts.Contract;

namespace SomeExtensions.Refactorings.SplitVariableInitializer {
	internal class SplitVariableInitializerRefactoring : IAsyncRefactoring {
		private readonly Document _document;
		private readonly VariableDeclarationSyntax _variableDeclaration;

		public SplitVariableInitializerRefactoring(VariableDeclarationSyntax variableDeclaration, Document document) {
			Requires(variableDeclaration != null);
			Requires(document != null);

			_variableDeclaration = variableDeclaration;
			_document = document;
		}

		public string Description => "Split variable declaration and assigment";

		public async Task<CompilationUnitSyntax> ComputeRoot(CompilationUnitSyntax root) {
			var variable = _variableDeclaration.Variables.First();

			var newVariableDeclaration = _variableDeclaration
				.WithVariables(variable.WithInitializer(null).ItemToSeparatedList())
				.WithType(await GetVariableType(variable));

			var codeBlock = _variableDeclaration.Parent.Parent.As<BlockSyntax>();

			var originalPosition = codeBlock.Statements.IndexOf(_variableDeclaration.Parent.As<StatementSyntax>());

			var variableIdentifier = variable.Identifier.Text.ToIdentifierName();

			var newStatements = codeBlock.Statements
				.RemoveAt(originalPosition)
				.Insert(originalPosition,
					newVariableDeclaration
						.ToLocalDeclaration()
						.Nicefy())
				.Insert(originalPosition + 1,
					variableIdentifier.AssignWith(variable.Initializer.Value).Nicefy());

			return root.ReplaceNode(
				codeBlock,
				codeBlock.WithStatements(newStatements));
		}

		private async Task<TypeSyntax> GetVariableType(VariableDeclaratorSyntax variable) {
			var type = _variableDeclaration.Type;

			if (type.IsVar) {
				var model = await _document.GetSemanticModelAsync(CancellationTokenExtensions.GetCancellationToken());
				type = model
					.GetSpeculativeExpressionType(variable.Initializer.Value)
					.ToTypeSyntax();
			}

			return type;
		}
	}
}
