using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

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

        public CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token) {
            var originalName = _parameter.Name;

            var parameterName = originalName.ToParameterName();
			var parameter = parameterName.ToParameter(_parameter.ParameterType);

			var assignment = originalName
				.ToIdentifierName(qualifyWithThis: originalName == parameterName)
				.AssignWith(parameterName.ToIdentifierName());

			var newCtor = _ctor
				.AddParameterListParameters(parameter)
                .WithBody(_ctor.Body.AddStatements(assignment));

            return root.ReplaceNode(_ctor, newCtor.Nicefy());
        }
    }
}
