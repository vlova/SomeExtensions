using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Extensions;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.FluentBuilder {
	// TODO: support of contracts
	// TODO: better support for collections
	// TODO: better support for dictionaries
	// TODO: ability to create builder from instance of object
	internal class CreateFluentBuilderRefactoring : IRefactoring {
		private readonly Document _document;
		private readonly TypeDeclarationSyntax _type;
		private readonly ConstructorDeclarationSyntax _constructor;

		public CreateFluentBuilderRefactoring(Document document, TypeDeclarationSyntax type, ConstructorDeclarationSyntax constructor) {
			_document = document;
			_type = type;
			_constructor = constructor;
		}

		public string Description { get; } = "Create fluent builder";

		protected IEnumerable<ParameterSyntax> _parameters {
			get {
				return _constructor.ParameterList.Parameters;
			}
		}

		public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
			return root
				.ReplaceNode(_type, GetNewTypeDeclaration())
				.As<CompilationUnitSyntax>()
				.AddUsingIfNotExists("System.Diagnostics");
		}

		private TypeDeclarationSyntax GetNewTypeDeclaration() {
			var oldBuilder = _type
				.ChildNodes()
				.OfType<TypeDeclarationSyntax>()
				.Where(t => t.Identifier.Text == "Builder")
				.FirstOrDefault();

			if (oldBuilder != null) {
				return _type.ReplaceNode(oldBuilder, GetBuilderClass());
			}
			else {
				return _type.InsertAfter(_type.ChildNodes().Last(), GetBuilderClass());
			}
		}

		private ClassDeclarationSyntax GetBuilderClass() {
			var fields = _parameters.Select(ToField);
			var methods = _parameters.Select(ToMethod);
			var members = fields
				.Concat<MemberDeclarationSyntax>(methods)
				.Concat(new[] { ToBuildMethod() });

			return SyntaxFactory
				.ClassDeclaration("Builder")
				.WithModifiers(_constructor.Modifiers)
				.WithMembers(members.ToSyntaxList())
				.WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, "\n")))
				.Nicefy();
		}

		private FieldDeclarationSyntax ToField(ParameterSyntax parameter) {
			return parameter.Identifier.Text.ToFieldName()
				.ToVariableDeclarator(initializer: parameter.Default)
				.ToVariableDeclaration(type: parameter.Type)
				.ToFieldDeclaration()
				.WithModifiers(SyntaxKind.PrivateKeyword)
				.Nicefy();
		}

		private static MethodDeclarationSyntax ToMethod(ParameterSyntax parameter) {
			var builderType = SyntaxFactory.ParseTypeName("Builder");

			return SyntaxFactory
				.MethodDeclaration(
					returnType: builderType,
					identifier: GetMethodName(parameter))
				.WithModifiers(SyntaxKind.PublicKeyword)
				.WithParameterList(GetMethodParameters(parameter))
				.WithBody(GetAssignMethodBody(parameter))
				.WithAttributeLists(GetMethodAttributes())
				.Nicefy();
		}

		private static string GetMethodName(ParameterSyntax parameter) {
			var name = parameter.Identifier.Text;
            var predefinedType = parameter.Type.As<PredefinedTypeSyntax>()
				?? parameter.Type.As<NullableTypeSyntax>().ElementType?.As<PredefinedTypeSyntax>();

			if (predefinedType?.Keyword.CSharpKind() == SyntaxKind.BoolKeyword) {
				return name.BoolParameterToMethodName();
            }

			return "With" + name.UppercaseFirst();
		}

		private static ParameterListSyntax GetMethodParameters(ParameterSyntax parameter) {
			return SyntaxFactory.ParameterList(parameter.ItemToSeparatedList());
		}

		private static BlockSyntax GetAssignMethodBody(ParameterSyntax parameter) {
			var parameterName = parameter.Identifier.Text;

			var methodParameter = parameterName.ToIdentifierName();
			var field = parameterName.ToFieldName().ToIdentifierName();

			var statements = new StatementSyntax[] {
				field.AssignWith(methodParameter).ToStatement(),
				SyntaxFactory.ThisExpression().ToReturnStatement()
			};

			return SyntaxFactory.Block(statements);
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

		private MethodDeclarationSyntax ToBuildMethod() {
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
			var typeSyntax = SyntaxFactory.ParseTypeName(_type.Identifier.Text);

			var ctorArguments = _parameters
				.Select(p => p.Identifier.Text.ToFieldName().ToIdentifierName())
				.ToArgumentList();

			var statements = new StatementSyntax[] {
				SyntaxFactory
					.ObjectCreationExpression(typeSyntax, ctorArguments, null)
					.ToReturnStatement()
			};

			return SyntaxFactory.Block(statements);
		}
	}
}
