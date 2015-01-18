using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace SomeExtensions.Refactorings {
	public interface IRefactoring {
		string Description { get; }

		SyntaxNode ComputeRoot(SyntaxNode root, CancellationToken token);
	}

	public interface IAsyncRefactoring {
		string Description { get; }

		Task<SyntaxNode> ComputeRoot(SyntaxNode root, CancellationToken token);
	}
}
