using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Extensions.Syntax {
	public static partial class SyntaxFactoryExtensions {
		public static ReturnStatementSyntax ToReturnStatement(this ExpressionSyntax expression) {
			return ReturnStatement(expression);
		}

		public static BlockSyntax ToBlock(this StatementSyntax statement) {
			return Block(statement);
		}

		public static BlockSyntax ToBlock(this IEnumerable<StatementSyntax> statements) {
			return Block(statements);
		}

		public static ExpressionStatementSyntax ToStatement(this ExpressionSyntax expr) {
			return ExpressionStatement(expr);
		}
	}
}
