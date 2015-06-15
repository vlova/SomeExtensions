using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Refactorings {
	public interface IRefactoring {
		string Description { get; }

		CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root);
	}

	public interface ISolutionRefactoring {
		string Title { get; }

		Task<Solution> ComputeRoot(Solution solution);
	}

	public interface IAsyncRefactoring {
		string Description { get; }

		Task<CompilationUnitSyntax> ComputeRoot(CompilationUnitSyntax root);
	}
}
