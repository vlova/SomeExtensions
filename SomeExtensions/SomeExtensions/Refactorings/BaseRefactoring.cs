using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace SomeExtensions.Refactorings {
    public abstract class BaseRefactoring : IRefactoring {
        private readonly Document _document;

        protected BaseRefactoring(Document document) {
            _document = document;
        }

        public abstract string Description { get; }

        protected Document Document {
            get {
                return _document;
            }
        }

        public async Task<Document> Compute(CancellationToken token) {
            return _document.WithSyntaxRoot(await ComputeRoot(token));
        }

        public async Task<SyntaxNode> ComputeRoot(CancellationToken token) {
            return await ComputeRoot(await _document.GetSyntaxRootAsync(token), token);
        }

        public async Task<SyntaxNode> ComputeRoot(SyntaxNode root, CancellationToken token) {
            try {
                return await ComputeRootInternal(root, token);
            }
            catch (OperationCanceledException) {
                throw;
            }
            catch (Exception) {
                return root;
                // TODO: add logging
            }
        }

        protected abstract Task<SyntaxNode> ComputeRootInternal(SyntaxNode root, CancellationToken token);
    }
}
