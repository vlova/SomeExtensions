using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        private static readonly MetadataReference corlibReference = MetadataReference.CreateFromAssembly(typeof(object).Assembly);
		private static readonly MetadataReference s_systemCoreReference = MetadataReference.CreateFromAssembly(typeof(Enumerable).Assembly);
		private static readonly MetadataReference s_CSharpSymbolsReference = MetadataReference.CreateFromAssembly(typeof(CSharpCompilation).Assembly);
		private static readonly MetadataReference s_codeAnalysisReference = MetadataReference.CreateFromAssembly(typeof(Compilation).Assembly);
		private static readonly MetadataReference s_immutableCollectionsReference = MetadataReference.CreateFromAssembly(typeof(ImmutableArray<int>).Assembly);


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
				var providerDirectory = GetProviderDirectory(provider);
				if (!Directory.Exists(providerDirectory)) {
					continue;
				}

				var casesDirectories = Directory.EnumerateDirectories(providerDirectory);
				foreach (var caseDirectory in casesDirectories) {
					var source = GetSourceContent(caseDirectory);

					foreach (var actionFilename in GetActionFilenames(caseDirectory)) {
						var lines = File.ReadAllLines(actionFilename);
						var actionTitle = GetActionTitle(lines);
						var actionResult = string.Join("\n", lines.Skip(1));
						yield return new object[] { actionTitle.Trim(), provider, source, actionResult };
					}
				}
			};
		}

		public static IEnumerable ActionsTitles() {
			var providers = Providers();
			foreach (var provider in providers) {
				var providerDirectory = GetProviderDirectory(provider);
				if (!Directory.Exists(providerDirectory)) {
					continue;
				}

				var casesDirectories = Directory.EnumerateDirectories(providerDirectory);
				foreach (var caseDirectory in casesDirectories) {
					var actionTitles = GetActionFilenames(caseDirectory)
						.Select(p => GetActionTitle(File.ReadAllLines(p)))
						.ToArray();

					yield return new object[] {
						provider,
						GetSourceContent(caseDirectory),
						caseDirectory.Substring(caseDirectory.LastIndexOf("\\")),
						actionTitles };
				}
			};
		}

		private static string GetSourceContent(string caseDirectory) {
			return File.ReadAllText(Path.Combine(caseDirectory, "Source.cs"));
		}

		private static string GetActionTitle(string[] lines) {
			return lines.First().Substring("//".Length).Trim();
		}

		private static IEnumerable<string> GetActionFilenames(string caseDirectory) {
			return Directory
				.EnumerateFiles(caseDirectory)
				.Where(r => r != Path.Combine(caseDirectory, "Source.cs"));
		}

		private static string GetProviderDirectory(CodeRefactoringProvider provider) {
			var dirName = provider.GetType().Name.Fluent(r => r.Substring(0, r.Length - "Provider".Length));
			var providerDirectory = Path.Combine(Directory.GetCurrentDirectory(), dirName);
			return providerDirectory;
		}

		private static void ApplyActions(CustomWorkspace workspace, CodeAction action, CancellationTokenSource cts) {
			var operations = action.GetOperationsAsync(cts.Token).Result;
			foreach (var operation in operations) {
				operation.Apply(workspace, cts.Token);
			}
		}

		private static string BadResultMessage(SyntaxTree actualTree) {
			return string.Format("Failed, produced source:\n {0}", actualTree.GetText().ToString());
		}

		private static string NotFoundMessage(IEnumerable<CodeAction> actions) {
			return string.Format("Code action not found (actions: {0})", string.Join(", ", actions.Select(a => a.Title)));
		}

		[Test, TestCaseSource("TestCases")]
		public void Test(string actionTitle, CodeRefactoringProvider provider, string source, string result) {
			var cursorPos = source.IndexOf(CursorSymbol);
			AreNotEqual(-1, cursorPos, "There are no cursor in test source");

			source = source.Replace(CursorSymbol, "");

			using (var workspace = new CustomWorkspace()) {
				var project = workspace.AddProject("TestProject", LanguageNames.CSharp)
					.AddMetadataReference(corlibReference)
					.AddMetadataReference(s_systemCoreReference);

				var document = project.AddDocument("TestCode.cs", source);

				var cts = new CancellationTokenSource();

				var actions = new List<CodeAction>();

				var context = new CodeRefactoringContext(
					document: document,
					span: TextSpan.FromBounds(start: cursorPos, end: cursorPos),
					registerRefactoring: a => { actions.Add(a); },
					cancellationToken: cts.Token);

				provider.ComputeRefactoringsAsync(context).Wait();

				var action = actions.SingleOrDefault(r => r.Title.Trim() == actionTitle.Trim());

				IsTrue(action != null, NotFoundMessage(actions));

				ApplyActions(workspace, action, cts);

				var newDocument = workspace.CurrentSolution.Projects.Single().Documents.Single();

				var expectedTree = SyntaxFactory.ParseCompilationUnit(result).SyntaxTree;
				var actualTree = newDocument.GetSyntaxTreeAsync(cts.Token).Result;

				IsTrue(
					condition: SyntaxFactory.AreEquivalent(expectedTree, actualTree, false),
					message: BadResultMessage(actualTree));
			}
		}

		[Test, TestCaseSource("ActionsTitles")]
		public void TestAllActionsCovered(CodeRefactoringProvider provider, string source, string caseName, IEnumerable<string> actionTitles) {
			var cursorPos = source.IndexOf(CursorSymbol);
			source = source.Replace(CursorSymbol, "");

			using (var workspace = new CustomWorkspace()) {
				var project = workspace.AddProject("TestProject", LanguageNames.CSharp)
					.AddMetadataReference(corlibReference)
					.AddMetadataReference(s_systemCoreReference);
;
				var document = project.AddDocument("TestCode.cs", source);

				var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

				var actions = new List<CodeAction>();

				var context = new CodeRefactoringContext(
					document: document,
					span: TextSpan.FromBounds(start: cursorPos, end: cursorPos),
					registerRefactoring: a => { actions.Add(a); },
					cancellationToken: cts.Token);

				provider.ComputeRefactoringsAsync(context).Wait();

				var notPresentActions = actionTitles.Except(actions.Select(a => a.Title)).ToList();

				CollectionAssert.IsEmpty(notPresentActions, "Provider: {0}, Case: {1}", provider.GetType().Name, caseName);
			}
		}
	}
}