﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.Contracts {
	// TODO: support of static import of contracts
	// TODO: support of arrow syntax for bodies
	internal abstract class ContractRefactoringProviderBase<TNode>
		: BaseRefactoringProvider<TNode> where TNode : SyntaxNode {
		protected override int? FindUpLimit => 3;

		protected virtual ApplyTo ApplyTo => ApplyTo.Everything;

		protected abstract ContractKind Kind { get; }
		protected abstract ContractParameter GetContractParameter(SemanticModel semanticModel, TNode parameter);
		protected abstract ExpressionSyntax GetParameterReference(TNode parameter);

		private bool IsAlreadyDefined(BlockSyntax body, TNode parameter) {
			var contracts = Kind == ContractKind.Require
				? body.Statements.FindContractRequires()
				: body.Statements.FindContractEnsures();

			return contracts
				.Select(contract => contract.GetFirstArgument())
				.Any(x => x.ContainsEquivalentNode(GetParameterReference(parameter)));
		}

		protected sealed override async Task ComputeRefactoringsAsync(RefactoringContext context, TNode parameter) {
			var bodies = GetMethodBodies(parameter);
			if (!bodies.Any()) return;

			if (bodies.Any(body => IsAlreadyDefined(body, parameter))) return;

			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
			var contractParameter = GetContractParameter(semanticModel, parameter);

			foreach (var provider in Helpers.Providers) {
				if (provider.CanRefactor(contractParameter)) {
					context.Register(new AddInOutContractRefactoring(
						bodies,
						contractParameter,
						provider,
						Kind));
				}
			}
		}

		private IEnumerable<BlockSyntax> GetMethodBodies(TNode parameter) {
			if (ApplyTo.HasFlag(ApplyTo.Method)) {
				var methodBody = parameter.FindUp<BaseMethodDeclarationSyntax>()?.Body;
				if (methodBody != null) yield return methodBody;
			}

			if (ApplyTo.HasFlag(ApplyTo.Getter)) {
				var getterBody = parameter.FindUp<BasePropertyDeclarationSyntax>()?.GetAccessor()?.Body;
				if (getterBody != null) yield return getterBody;
			}

			if (ApplyTo.HasFlag(ApplyTo.Setter)) {
				var setterBody = parameter.FindUp<BasePropertyDeclarationSyntax>()?.SetAccessor()?.Body;
				if (setterBody != null) yield return setterBody;
			}
		}
	}
}