using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static ReturnStatementSyntax ToReturnStatement(this ExpressionSyntax expression) {
			return SyntaxFactory.ReturnStatement(expression);
		}

		public static BlockSyntax ToBlock(this StatementSyntax statement) {
			return SyntaxFactory.Block(statement);
		}

		public static BlockSyntax ToBlock(this IEnumerable<StatementSyntax> statements) {
			return SyntaxFactory.Block(statements);
		}

		public static ExpressionStatementSyntax ToStatement(this ExpressionSyntax expr) {
			return SyntaxFactory.ExpressionStatement(expr);
		}
	}
}
