﻿using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace SomeExtensions.Refactorings {
    public interface IRefactoring {
        string Description { get; }

        Task<Document> Compute(CancellationToken token);

        Task<SyntaxNode> ComputeRoot(SyntaxNode root, CancellationToken token);
    }
}
