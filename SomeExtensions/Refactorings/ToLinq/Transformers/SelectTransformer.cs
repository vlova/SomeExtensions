using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SomeExtensions.Extensions;
using SomeExtensions.Extensions.Syntax;

namespace SomeExtensions.Refactorings.ToLinq.Transformers {
	//internal class SelectTransformer : ILinqTransformer {
	//	private readonly ForEachStatementSyntax _foreach;

	//	public SelectTransformer(ForEachStatementSyntax @foreach) {
	//		_foreach = @foreach;
	//	}

	//	public string Description => "To linq";

	//	private bool ContainsSingleAddStatement() {
	//		return FindAddStatements().HasNotMore(1);
	//	}

	//	private IEnumerable<InvocationExpressionSyntax> FindAddStatements() {
	//		return _foreach.Statement.DescendantNodes<InvocationExpressionSyntax>()
	//			.Where(i => i.GetClassName() != null)
	//			.Where(i => i.GetMethodName() == "Add");
	//	}

	//	public bool CanTransform(CompilationUnitSyntax root) {
	//		return false;
	//	}

	//	public Tuple<CompilationUnitSyntax, ForEachStatementSyntax> Transform(CompilationUnitSyntax root, CancellationToken token) {
	//		throw new NotImplementedException();
	//	}
	//}
}