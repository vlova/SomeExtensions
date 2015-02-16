using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Refactorings {
	public interface IRefactoring {
		string Description { get; }

		CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token);
	}

	public interface IRefactoringWithOptions<TOptions> {
		string Description { get; }

		TOptions GetOptions();

		Task<CompilationUnitSyntax> ComputeRoot(TOptions options, CompilationUnitSyntax root, CancellationToken token);
	}

	public interface ISolutionRefactoring {
		string Description { get; }

		Task<Solution> ComputeRoot(Solution solution, CancellationToken token);
	}

	public interface IAsyncRefactoring {
		string Description { get; }

		Task<CompilationUnitSyntax> ComputeRoot(CompilationUnitSyntax root, CancellationToken token);
	}
}
