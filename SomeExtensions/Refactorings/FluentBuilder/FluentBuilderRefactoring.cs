using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Extensions;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;
using System.Diagnostics.Contracts;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Refactorings.FluentBuilder {
	// TODO: support of contracts
	// TODO: better support for collections
	// TODO: better support for dictionaries
	internal class FluentBuilderRefactoring : IRefactoring {
		private readonly TypeDeclarationSyntax _type;
		private readonly ConstructorDeclarationSyntax _constructor;

		public FluentBuilderRefactoring(TypeDeclarationSyntax type, ConstructorDeclarationSyntax constructor) {
			Contract.Requires(type != null);
			Contract.Requires(constructor != null);

			_type = type;
			_constructor = constructor;
		}

		public string Description => "Create fluent builder";

		#region helpers

		protected IEnumerable<ParameterSyntax> _parameters => _constructor.ParameterList.Parameters;

		private TypeSyntax GetOriginalTypeSyntax() {
			return ParseTypeName(_type.Identifier.Text);
		}

		private static SyntaxList<AttributeListSyntax> GetMethodAttributes() {
			return AttributeList(
					Attribute("DebuggerStepThrough".ToIdentifierName())
					.Nicefy()
					.ItemToSeparatedList())
				.ItemToSyntaxList();
		}

		#endregion

		public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
			return root
				.ReplaceNode(_type, GetNewTypeDeclaration())
				.AddUsingIfNotExists("System.Diagnostics");
		}

		private TypeDeclarationSyntax GetNewTypeDeclaration() {
			var oldBuilder = _type
				.ChildNodes()
				.OfType<TypeDeclarationSyntax>()
				.Where(t => t.Identifier.Text == "Builder")
				.FirstOrDefault();

			var newBuilder = GetBuilderClass();

			if (oldBuilder != null) {
				return _type.ReplaceNode(oldBuilder, newBuilder);
			}
			else {
				return _type.InsertAfter(_type.ChildNodes().Last(), newBuilder);
			}
		}

		private ClassDeclarationSyntax GetBuilderClass() {
			var members = Enumerable.Empty<MemberDeclarationSyntax>()
				.Concat(_parameters.Select(GetBuilderField))
				.Concat(_parameters.Select(GetAssigmentMethod))
				.Append(GetBuildMethod())
				.Append(ToOriginalTypeConversion())
				.Append(ToBuilderTypeConversion());

			return ClassDeclaration("Builder")
				.WithModifiers(PublicKeyword)
				.WithMembers(members.ToSyntaxList())
				.WithLeadingEndLine()
				.Nicefy();
		}

		private FieldDeclarationSyntax GetBuilderField(ParameterSyntax parameter) {
			var fieldName = parameter.Identifier.Text.ToFieldName();

			return fieldName
				.ToVariableDeclaration(type: parameter.Type, value: parameter.Default?.Value)
				.ToFieldDeclaration()
				.WithModifiers(PrivateKeyword)
				.Nicefy();
		}

		private MethodDeclarationSyntax GetAssigmentMethod(ParameterSyntax parameter) {
			return MethodDeclaration(
					returnType: "Builder".ToIdentifierName(),
					identifier: GetAssigmentMethodName(parameter))
				.WithModifiers(PublicKeyword)
				.WithParameterList(GetAssigmentMethodParameters(parameter))
				.WithBody(GetAssigmentMethodBody(parameter))
				.WithAttributeLists(GetMethodAttributes())
				.Nicefy();
		}

		private static string GetAssigmentMethodName(ParameterSyntax parameter) {
			var name = parameter.Identifier.Text;
			var predefinedType = parameter.Type.As<PredefinedTypeSyntax>()
				?? parameter.Type.As<NullableTypeSyntax>().ElementType?.As<PredefinedTypeSyntax>();

			if (predefinedType?.Keyword.CSharpKind() == BoolKeyword) {
				return name.BoolParameterToMethodName();
			}

			return "With" + name.UppercaseFirst();
		}

		private ParameterListSyntax GetAssigmentMethodParameters(ParameterSyntax parameter) {
			var parameterName = parameter.Identifier.Text;

			return parameterName
				.ToParameter(parameter.Type, parameter.Default?.Value)
				.ItemToSeparatedList()
				.ToParameterList();
		}

		private static BlockSyntax GetAssigmentMethodBody(ParameterSyntax parameter) {
			var parameterName = parameter.Identifier.Text;

			var methodParameter = parameterName.ToIdentifierName();
			var field = parameterName.ToFieldName().ToIdentifierName();

			var statements = new StatementSyntax[] {
				field.AssignWith(methodParameter),
				ReturnStatement(ThisExpression())
			};

			return statements.ToBlock();
		}

		private MethodDeclarationSyntax GetBuildMethod() {
			var returnType = ParseTypeName(_type.Identifier.Text);

			return MethodDeclaration(returnType: returnType, identifier: "Build")
				.WithModifiers(PublicKeyword)
				.WithParameterList(ParameterList())
				.WithBody(GetBuildMethodBody())
				.WithAttributeLists(GetMethodAttributes())
				.Nicefy();
		}

		private BlockSyntax GetBuildMethodBody() {
			var ctorArguments = _parameters
				.Select(p => p.Identifier.Text.ToFieldName().ToIdentifierName())
				.ToArgumentList();

			var builder = ObjectCreationExpression(GetOriginalTypeSyntax(), ctorArguments, null);


			return Block(ReturnStatement(builder));
		}

		private ConversionOperatorDeclarationSyntax ToOriginalTypeConversion() {
			var parameters = new[] {
				"argument".ToParameter("Builder".ToIdentifierName())
			};

			var statements = new StatementSyntax[] {
				ReturnStatement("argument".AccessTo("Build").ToInvocation())
			};

			return ConversionOperatorDeclaration(
					ImplicitKeyword.ToToken(),
					GetOriginalTypeSyntax())
				.WithParameterList(parameters.ToParameterList())
				.WithModifiers(PublicKeyword, StaticKeyword)
				.WithAttributeLists(GetMethodAttributes())
				.WithBody(Block(statements));
		}

		private ConversionOperatorDeclarationSyntax ToBuilderTypeConversion() {
			var parameters = new[] {
				"argument".ToParameter(GetOriginalTypeSyntax())
			};

			return ConversionOperatorDeclaration(
					ImplicitKeyword.ToToken(),
					"Builder".ToIdentifierName())
				.WithParameterList(parameters.ToParameterList())
				.WithModifiers(PublicKeyword, StaticKeyword)
				.WithAttributeLists(GetMethodAttributes())
				.WithBody(GetBuilderTypeStatements().ToBlock());
		}

		private IEnumerable<StatementSyntax> GetBuilderTypeStatements() {
			var builderCreator = ObjectCreationExpression("Builder".ToIdentifierName())
				.WithArgumentList(ArgumentList());

			yield return "builder"
				.ToVariableDeclaration("Builder".ToIdentifierName(), builderCreator)
				.ToLocalDeclaration();

			var assigments = _constructor.Body.DescendantNodes<AssignmentExpressionSyntax>();

			foreach (var assigment in assigments) {
				var sourceName = GetSourceName(assigment.Left);
				var setterMethod = GetBuilderSetterMethod(assigment.Right);

				if (sourceName == null || setterMethod == null) {
					continue;
				}

				yield return "builder".ToIdentifierName()
					.AccessTo(setterMethod)
					.ToInvocation(sourceName)
					.ToStatement();
			};

			yield return "builder".ToIdentifierName().ToReturnStatement();
		}

		private ExpressionSyntax GetSourceName(ExpressionSyntax assigneeExpression) {
			string assignee = assigneeExpression
				.As<IdentifierNameSyntax>()
				?.Identifier.Text;

			string assigneeOfThis = assigneeExpression
				.As<MemberAccessExpressionSyntax>()
				.Unless(m => m?.Expression is ThisExpressionSyntax)
				?.Name?.Identifier.Text;

			assignee = assignee ?? assigneeOfThis;

			if (assignee == null) {
				return null;
			}

			return "argument".ToIdentifierName().AccessTo(assignee);
		}

		private string GetBuilderSetterMethod(ExpressionSyntax assigment) {
			var nodes = assigment.DescendantNodes<IdentifierNameSyntax>();

			if (assigment is IdentifierNameSyntax) {
				nodes = nodes.Prepend(assigment.As<IdentifierNameSyntax>());
			}

			var parameter = nodes
				.Select(p => p.Identifier.Text)
				.Select(p => _parameters.FirstOrDefault(q => q.Identifier.Text == p))
				.FirstOrDefault(p => p != null);

			return GetAssigmentMethodName(parameter);
		}
	}
}
