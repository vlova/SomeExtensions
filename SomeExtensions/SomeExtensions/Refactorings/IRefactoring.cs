using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace SomeExtensions.Refactorings {
	// TODO: rename it to IAsyncRefactoring & provide IRefactoring class
	public interface IRefactoring {
		string Description { get; }

		Task<SyntaxNode> ComputeRoot(SyntaxNode root, CancellationToken token);
	}
}
