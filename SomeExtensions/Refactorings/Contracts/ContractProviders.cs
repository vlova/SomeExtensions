using System.Composition;
using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions.Semantic;
using SomeExtensions.Extensions.Syntax;
using static Microsoft.CodeAnalysis.LanguageNames;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.Contracts {
	[ExportCodeRefactoringProvider(nameof(RequiresParameterProvider), CSharp), Shared]
	internal class RequiresParameterProvider : ContractRefactoringProviderBase<ParameterSyntax> {
		protected override ContractKind Kind => ContractKind.Require;

		protected override ExpressionSyntax GetParameterReference(ParameterSyntax parameter) {
			return parameter.Identifier.Text.ToIdentifierName();
		}

		protected override ContractParameter GetContractParameter(SemanticModel semanticModel, ParameterSyntax parameter) {
			return new ContractParameter(
				parameter.Identifier.Text,
				GetParameterReference(parameter),
				parameter?.Default?.Value,
				semanticModel.GetTypeSymbol(parameter.Type)
			);
		}
	}

	[ExportCodeRefactoringProvider(nameof(RequiresSetterValueProvider), CSharp), Shared]
	internal class RequiresSetterValueProvider : ContractRefactoringProviderBase<BasePropertyDeclarationSyntax> {
		protected override ContractKind Kind => ContractKind.Require;
		protected override ApplyTo ApplyTo => ApplyTo.Setter;

		protected override ExpressionSyntax GetParameterReference(BasePropertyDeclarationSyntax parameter) {
			return "value".ToIdentifierName();
		}

		protected override ContractParameter GetContractParameter(SemanticModel semanticModel, BasePropertyDeclarationSyntax parameter) {
			return new ContractParameter(
				"value",
				GetParameterReference(parameter),
				null,
				semanticModel.GetTypeSymbol(parameter.Type));
		}
	}

	// TODO: only apply for out parameters
	[ExportCodeRefactoringProvider(nameof(EnsuresOutParameterProvider), CSharp), Shared]
	internal class EnsuresOutParameterProvider : ContractRefactoringProviderBase<ParameterSyntax> {
		protected override ContractKind Kind => ContractKind.Ensure;

		protected override ContractParameter GetContractParameter(SemanticModel semanticModel, ParameterSyntax parameter) {
			return new ContractParameter(
				$"out {parameter.Identifier.Text}",
				GetParameterReference(parameter),
				parameter?.Default?.Value,
				semanticModel.GetTypeSymbol(parameter.Type)
			);
		}

		protected override ExpressionSyntax GetParameterReference(ParameterSyntax parameter) {
			var parameterName = parameter.Identifier.Text.ToIdentifierName();
			return "Contract.ValueAtReturn"
				.ToInvocation(Argument(parameterName).WithOutKeyword());
		}
	}

	[ExportCodeRefactoringProvider(nameof(EnsuresGetterResultProvider), CSharp), Shared]
	internal class EnsuresGetterResultProvider : ContractRefactoringProviderBase<BasePropertyDeclarationSyntax> {
		protected override int? FindUpLimit => 5;
		protected override ContractKind Kind => ContractKind.Ensure;
		protected override ApplyTo ApplyTo => ApplyTo.Getter;

		protected override ExpressionSyntax GetParameterReference(BasePropertyDeclarationSyntax property) {
			return nameof(Contract)
				.AccessTo(nameof(Contract.Result).MakeGeneric(property.Type))
				.ToInvocation();
		}

		protected override ContractParameter GetContractParameter(SemanticModel semanticModel, BasePropertyDeclarationSyntax property) {
			return new ContractParameter(
				"result",
				GetParameterReference(property),
				defaultValue: null,
				type: semanticModel.GetTypeSymbol(property.Type)
			);
		}
	}

	[ExportCodeRefactoringProvider(nameof(EnsuresMethodResultProvider), CSharp), Shared]
	internal class EnsuresMethodResultProvider : ContractRefactoringProviderBase<MethodDeclarationSyntax> {
		protected override ContractKind Kind => ContractKind.Ensure;
		protected override ApplyTo ApplyTo => ApplyTo.Method;

		protected override ExpressionSyntax GetParameterReference(MethodDeclarationSyntax method) {
			return nameof(Contract)
				.AccessTo(nameof(Contract.Result).MakeGeneric(method.ReturnType))
				.ToInvocation();
		}

		protected override ContractParameter GetContractParameter(SemanticModel semanticModel, MethodDeclarationSyntax method) {
			return new ContractParameter(
				"result",
				GetParameterReference(method),
				defaultValue: null,
				type: semanticModel.GetTypeSymbol(method.ReturnType)
			);
		}
	}
}
