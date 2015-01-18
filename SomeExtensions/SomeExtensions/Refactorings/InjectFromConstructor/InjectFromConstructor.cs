using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;

namespace SomeExtensions.Refactorings.InjectFromConstructor {
	internal struct InjectFromConstructor : IRefactoring {
        private readonly InjectParameter _parameter;
        private readonly ConstructorDeclarationSyntax _ctor;
        private readonly int? _ctorNo;

        public InjectFromConstructor(InjectParameter parameter, ConstructorDeclarationSyntax ctor, int? ctorNo = null) {
            _parameter = parameter;
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
            var fieldName = _parameter.Name;
            var parameterName = fieldName.ToParameterName();

            var newCtor = _ctor
                .AddParameterListParameters(
                    parameterName.ToParameter(_parameter.ParameterType))
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
