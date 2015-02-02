using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using SomeExtensions.Extensions;
using SomeExtensions.Refactorings.InjectFromConstructor;
using static NUnit.Framework.Assert;

namespace Tests {
	[TestFixture]
	public class RefactoringsTests {
		const string CursorSymbol = "º";

		public static IEnumerable<CodeRefactoringProvider> Providers() {
			var exportType = typeof(ExportCodeRefactoringProviderAttribute);
			var providers = typeof(InjectFromConstructorProvider)
				.Assembly.DefinedTypes
				.Where(r => typeof(CodeRefactoringProvider).IsAssignableFrom(r))
				.Where(r => r.CustomAttributes.Any(a => a.AttributeType == exportType))
				.Select(r => Activator.CreateInstance(r))
				.Cast<CodeRefactoringProvider>()
				.ToArray();

            return providers;
		}

		public static IEnumerable TestCases() {
			var providers = Providers();
			foreach (var provider in providers) {
				var dirName = provider.GetType().Name.Fluent(r => r.Substring(0, r.Length - "Provider".Length));
				var providerDirectory = Path.Combine(Directory.GetCurrentDirectory(), dirName);
                if (!Directory.Exists(providerDirectory)) {
					continue;
				}

				var cases = Directory.EnumerateDirectories(providerDirectory);
				foreach (var caseDirectory in cases) {
					var results = Directory
						.EnumerateFiles(caseDirectory)
						.Where(r => r != Path.Combine(caseDirectory, "Source.cs"))
						.ToArray();

					foreach (var resultFile in results) {
						var source = File.ReadAllText(Path.Combine(caseDirectory, "Source.cs"));
						var lines = File.ReadAllLines(resultFile);
						var resultTitle = lines.First().Substring("//".Length);
						var resultSource = string.Join("\n", lines.Skip(1));
						yield return new object[] { provider, source, resultTitle, resultSource };
					}
				}
            };
		}

		[Test, TestCaseSource("TestCases")]
		public void Test(CodeRefactoringProvider provider, string source, string actionTitle, string result) {
			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
			var cursorPos = source.IndexOf(CursorSymbol);
			source = source.Replace(CursorSymbol, "");

			using (var workspace = new CustomWorkspace()) {
				var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
				var document = project.AddDocument("TestCode.cs", source);

				var actions = new List<CodeAction>();

				var context = new CodeRefactoringContext(
					document: document,
					span: TextSpan.FromBounds(start: cursorPos, end: cursorPos),
					registerRefactoring: a => { actions.Add(a); },
					cancellationToken: cts.Token);

				provider.ComputeRefactoringsAsync(context).Wait();

				var action = actions.SingleOrDefault(r => r.Title.Trim() == actionTitle.Trim());

				IsTrue(action != null, "Code action not found");

				var operations = action.GetOperationsAsync(cts.Token).Result;
				foreach (var operation in operations) {
					operation.Apply(workspace, cts.Token);
				}

				var newDocument = workspace.CurrentSolution.Projects.Single().Documents.Single();

				var expectedTree = SyntaxFactory.ParseCompilationUnit(result).SyntaxTree;
                var actualTree = newDocument.GetSyntaxTreeAsync(cts.Token).Result;

				IsTrue(SyntaxFactory.AreEquivalent(expectedTree, actualTree, false), "Failed");
			}
        }
	}
}