using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
    internal struct InjectFromConstructor : IRefactoring {
        private readonly FieldDeclarationSyntax _field;
        private readonly ConstructorDeclarationSyntax _ctor;
        private readonly int? _ctorNo;

        public InjectFromConstructor(FieldDeclarationSyntax field, ConstructorDeclarationSyntax ctor, int? ctorNo = null) {
            _field = field;
            _ctor = ctor;
            _ctorNo = ctorNo;
        }

        public string Description {
            get {
                if (_ctorNo == null) {
                    return "Inject from constructor";
                }
                else {
                    return "Inject from constructor #" + _ctorNo;
                }
            }
        }

        public SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token) {
            var varDecl = _field.Declaration.Variables.FirstOrDefault();
            var fieldName = varDecl.Identifier.Text;
            var parameterName = fieldName.ToParameterName();

            var newCtor = _ctor
                .AddParameterListParameters(
                    parameterName.ToParameter(_field.Declaration.Type))
                .WithBody(_ctor.Body.AddStatements(
                    fieldName
                        .ToIdentifierName(qualifyWithThis: fieldName == parameterName)
                        .AssignWith(parameterName.ToIdentifierName())
                        .ToStatement()))
                .Nicefy();

            return root.ReplaceNode(_ctor, newCtor);
        }
    }
}
