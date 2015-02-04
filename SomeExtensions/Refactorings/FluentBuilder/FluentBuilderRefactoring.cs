using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Extensions;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.FluentBuilder {
	// TODO: support of contracts
	// TODO: better support for collections
	// TODO: better support for dictionaries
	internal class FluentBuilderRefactoring : IRefactoring {
		private readonly Document _document;
		private readonly TypeDeclarationSyntax _type;
		private readonly ConstructorDeclarationSyntax _constructor;

		public FluentBuilderRefactoring(Document document, TypeDeclarationSyntax type, ConstructorDeclarationSyntax constructor) {
			_document = document;
			_type = type;
			_constructor = constructor;
		}

		public string Description => "Create fluent builder";

		# region helpers

		protected IEnumerable<ParameterSyntax> _parameters {
			get {
				return _constructor.ParameterList.Parameters;
			}
		}

		private TypeSyntax GetOriginalTypeSyntax() {
			return SyntaxFactory.ParseTypeName(_type.Identifier.Text);
		}

		private static SyntaxList<AttributeListSyntax> GetMethodAttributes() {
			return SyntaxFactory
				.AttributeList(
					SyntaxFactory
						.Attribute("DebuggerStepThrough".ToIdentifierName())
						.Nicefy()
						.ItemToSeparatedList())
				.ItemToSyntaxList();
		}

		#endregion

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
			var newRoot = root
				.ReplaceNode(_type, GetNewTypeDeclaration())
				.As<CompilationUnitSyntax>()
				.AddUsingIfNotExists("System.Diagnostics");

			return newRoot;
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
				return _type.InsertAfter(_type.ChildNodes().Last(), GetBuilderClass());
			}
		}

		private ClassDeclarationSyntax GetBuilderClass() {
			var members = Enumerable.Empty<MemberDeclarationSyntax>()
				.Concat(_parameters.Select(GetBuilderField))
				.Concat(_parameters.Select(GetAssigmentMethod))
				.Append(GetBuildMethod())
				.Append(ToOriginalTypeConversion())
				.Append(ToBuilderTypeConversion());

			return SyntaxFactory
				.ClassDeclaration("Builder")
				.WithModifiers(SyntaxKind.PublicKeyword)
				.WithMembers(members.ToSyntaxList())
				.WithLeadingEndLine()
				.Nicefy();
		}

		private ExpressionSyntax GetDefaultValue(ParameterSyntax parameter) {
			return parameter?.Default?.Value?.ToFullString()?.ParseExpression();
        }

		private TypeSyntax GetType(ParameterSyntax parameter) {
			return parameter.Type.ToFullString().ParseTypeName();
		}

		private FieldDeclarationSyntax GetBuilderField(ParameterSyntax parameter) {
			var fieldName = parameter.Identifier.Text.ToFieldName();

			return fieldName
				.ToVariableDeclaration(type: GetType(parameter), value: GetDefaultValue(parameter))
				.ToFieldDeclaration()
				.WithModifiers(SyntaxKind.PrivateKeyword)
				.Nicefy();
		}

		private MethodDeclarationSyntax GetAssigmentMethod(ParameterSyntax parameter) {
			return SyntaxFactory.MethodDeclaration(
					returnType: "Builder".ToIdentifierName(),
					identifier: GetAssigmentMethodName(parameter))
				.WithModifiers(SyntaxKind.PublicKeyword)
				.WithParameterList(GetAssigmentMethodParameters(parameter))
				.WithBody(GetAssigmentMethodBody(parameter))
				.WithAttributeLists(GetMethodAttributes())
				.Nicefy();
		}

		private static string GetAssigmentMethodName(ParameterSyntax parameter) {
			var name = parameter.Identifier.Text;
            var predefinedType = parameter.Type.As<PredefinedTypeSyntax>()
				?? parameter.Type.As<NullableTypeSyntax>().ElementType?.As<PredefinedTypeSyntax>();

			if (predefinedType?.Keyword.CSharpKind() == SyntaxKind.BoolKeyword) {
				return name.BoolParameterToMethodName();
            }

			return "With" + name.UppercaseFirst();
		}

		private ParameterListSyntax GetAssigmentMethodParameters(ParameterSyntax parameter) {
			var parameterName = parameter.Identifier.Text;

            return parameterName
				.ToParameter(GetType(parameter), GetDefaultValue(parameter))
				.ItemToSeparatedList()
				.ToParameterList();
		}

		private static BlockSyntax GetAssigmentMethodBody(ParameterSyntax parameter) {
			var parameterName = parameter.Identifier.Text;

			var methodParameter = parameterName.ToIdentifierName();
			var field = parameterName.ToFieldName().ToIdentifierName();

			var statements = new StatementSyntax[] {
				field.AssignWith(methodParameter),
				SyntaxFactory.ThisExpression().ToReturnStatement()
			};

			return statements.ToBlock();
		}

		private MethodDeclarationSyntax GetBuildMethod() {
			var returnType = SyntaxFactory.ParseTypeName(_type.Identifier.Text);

			return SyntaxFactory
				.MethodDeclaration(returnType: returnType, identifier: "Build")
				.WithModifiers(SyntaxKind.PublicKeyword)
				.WithParameterList(SyntaxFactory.ParameterList())
				.WithBody(GetBuildMethodBody())
				.WithAttributeLists(GetMethodAttributes())
				.Nicefy();
		}

		private BlockSyntax GetBuildMethodBody() {
			var ctorArguments = _parameters
				.Select(p => p.Identifier.Text.ToFieldName().ToIdentifierName())
				.ToArgumentList();

			var statements = new StatementSyntax[] {
				SyntaxFactory
					.ObjectCreationExpression(GetOriginalTypeSyntax(), ctorArguments, null)
					.ToReturnStatement()
			};

			return SyntaxFactory.Block(statements);
		}

		private ConversionOperatorDeclarationSyntax ToOriginalTypeConversion() {
			var parameters = new[] {
				"argument".ToParameter("Builder".ToIdentifierName())
			};

			var statements = new StatementSyntax[] {
				"argument".AccessTo("Build").ToInvocation().ToReturnStatement()
            };

			return SyntaxFactory
				.ConversionOperatorDeclaration(
					SyntaxKind.ImplicitKeyword.ToToken(),
					GetOriginalTypeSyntax())
				.WithParameterList(parameters.ToParameterList())
				.WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
				.WithAttributeLists(GetMethodAttributes())
				.WithBody(SyntaxFactory.Block(statements));
		}

		private ConversionOperatorDeclarationSyntax ToBuilderTypeConversion() {
			var parameters = new[] {
				"argument".ToParameter(GetOriginalTypeSyntax())
			};

			return SyntaxFactory
				.ConversionOperatorDeclaration(
					SyntaxKind.ImplicitKeyword.ToToken(),
					"Builder".ToIdentifierName())
				.WithParameterList(parameters.ToParameterList())
				.WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
				.WithAttributeLists(GetMethodAttributes())
				.WithBody(GetBuilderTypeStatements().ToBlock());
		}

		private IEnumerable<StatementSyntax> GetBuilderTypeStatements() {
			var builderCreator = SyntaxFactory
				.ObjectCreationExpression("Builder".ToIdentifierName())
				.WithArgumentList(SyntaxFactory.ArgumentList());

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
