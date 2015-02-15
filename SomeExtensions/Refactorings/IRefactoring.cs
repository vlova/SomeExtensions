using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Refactorings {
	public interface IRefactoring {
		string Description { get; }

		CompilationUnitSyntax ComputeRoot(CompilationUnitSyntax root, CancellationToken token);
	}

	struct DocumentChange {
		public DocumentId Id { get; }
		public SyntaxNode NewRoot { get; }

		public DocumentChange(DocumentId id, SyntaxNode newRoot) {
			Id = id;
			NewRoot = newRoot;
		}
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
