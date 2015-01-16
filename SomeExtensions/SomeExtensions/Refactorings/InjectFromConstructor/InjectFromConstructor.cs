using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
    internal class InjectFromConstructor : BaseRefactoring {
        private readonly FieldDeclarationSyntax _field;
        private readonly ConstructorDeclarationSyntax _ctor;
        private readonly int? _ctorNo;

        public InjectFromConstructor(Document document, FieldDeclarationSyntax field, ConstructorDeclarationSyntax ctor, int? ctorNo = null) : base(document) {
            _field = field;
            _ctor = ctor;
            _ctorNo = ctorNo;
        }

        public override string Description {
            get {
                if (_ctorNo == null) {
                    return "Inject from constructor";
                }
                else {
                    return "Inject from constructor #" + _ctorNo;
                }
            }
        }

        protected override async Task<SyntaxNode> ComputeRootInternal(SyntaxNode root, CancellationToken token) {
            var varDecl = _field.Declaration.Variables.FirstOrDefault();
            var fieldName = varDecl.Identifier.Text;
            var parameterName = fieldName.ToParameterName();

            var newCtor = _ctor
                .AddParameterListParameters(
                    parameterName.ToParameter(_field.Declaration.Type))
                .WithBody(_ctor.Body.AddStatements(
                    fieldName
                        .ToIdentifier(qualifyWithThis: fieldName == parameterName)
                        .AssignWith(parameterName.ToIdentifier())
                        .ToStatement()))
                .Nicefy();

            return root.ReplaceNode(_ctor, newCtor);
        }
    }

}
