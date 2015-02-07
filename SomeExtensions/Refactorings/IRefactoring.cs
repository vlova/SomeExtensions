using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Refactorings {
	public interface IRefactoring {
		string Description { get; }

		CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token);
	}

	public interface IAsyncRefactoring {
		string Description { get; }

		Task<CompilationUnitSyntax> ComputeRoot(CompilationUnitSyntax root, CancellationToken token);
	}
}
