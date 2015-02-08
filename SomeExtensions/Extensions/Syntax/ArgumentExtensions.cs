using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SomeExtensions.Extensions.Syntax {
	static class ArgumentExtensions {
		public static ArgumentSyntax WithNameColon(this ArgumentSyntax argument, string nameColon) {
			return argument.WithNameColon(NameColon(nameColon));
		}
	}
}
