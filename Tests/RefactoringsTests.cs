using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using SomeExtensions;
using SomeExtensions.Extensions;
using SomeExtensions.Refactorings.InjectFromConstructor;
using static Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions;
using static Microsoft.CodeAnalysis.Formatting.FormattingOptions;
using static Microsoft.CodeAnalysis.LanguageNames;
using static NUnit.Framework.Assert;

namespace Tests {
    // This class is used to test refacoting providers using files
    // It automatically searches cases in the directory that corresponds to provider name
    // Each case is a directory with files Source.cs and result files
    // Source file must contain marker symbol º that indicates cursor position
    // Result file must contain code action name as first line as the comment
    // If CodeRefactoringProvider provides CodeAction which is not described in any result file, then one of tests will fail
    // Be sure that files are properly formatted due to the options setted in the GetWorkspace() method
    [TestFixture]
    public class RefactoringsTests {
        private static readonly MetadataReference corlibReference
            = MetadataReference.CreateFromAssembly(typeof(object).Assembly);
        private static readonly MetadataReference s_systemCoreReference
            = MetadataReference.CreateFromAssembly(typeof(Enumerable).Assembly);
        private static readonly MetadataReference s_CSharpSymbolsReference
            = MetadataReference.CreateFromAssembly(typeof(CSharpCompilation).Assembly);
        private static readonly MetadataReference s_codeAnalysisReference
            = MetadataReference.CreateFromAssembly(typeof(Compilation).Assembly);
        private static readonly MetadataReference s_immutableCollectionsReference
            = MetadataReference.CreateFromAssembly(typeof(ImmutableArray<int>).Assembly);

        private const string CursorSymbol = "º";

        [SetUp]
        public void Init() {
            Settings.Instance.CanThrow = true;
        }

        public static IEnumerable<RefactoringProvider> Providers() {
            var exportType = typeof(ExportCodeRefactoringProviderAttribute);
            var providers = typeof(InjectFromConstructorProvider)
                .Assembly.DefinedTypes
                .Where(r => typeof(CodeRefactoringProvider).IsAssignableFrom(r))
                .Where(r => r.CustomAttributes.Any(a => a.AttributeType == exportType))
                .Select(r => new RefactoringProvider(Activator.CreateInstance(r) as CodeRefactoringProvider))
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

                var casesDirectories = GetCasesDirectories(providerDirectory);
                foreach (var caseDirectory in casesDirectories) {
                    var sourcePath = caseDirectory.F(GetSourcePath);

                    foreach (var actionFilename in GetActionFilenames(caseDirectory)) {
                        var lines = File.ReadLines(actionFilename);
                        var actionTitle = GetActionTitle(lines);
                        yield return
                            new TestCaseData(
                                provider,
                                caseDirectory.Substring(providerDirectory.Length),
                                actionTitle.Trim(),
                                sourcePath,
                                actionFilename)
                            .SetProperty("Refactoring", provider.Provider.GetType().Name);
                    }
                }
            };
        }

        private static IEnumerable<string> GetCasesDirectories(string directory) {
            foreach (var caseDirectory in Directory.EnumerateDirectories(directory)) {
                if (caseDirectory.F(GetSourcePath).F(File.Exists)) {
                    yield return caseDirectory;
                }

                foreach (var childDirectory in GetCasesDirectories(caseDirectory)) {
                    yield return childDirectory;
                }
            }
        }

        public static IEnumerable ActionsTitles() {
            var providers = Providers();
            foreach (var provider in providers) {
                var providerDirectory = GetProviderDirectory(provider);
                if (!Directory.Exists(providerDirectory)) {
                    continue;
                }

                var casesDirectories = GetCasesDirectories(providerDirectory);
                foreach (var caseDirectory in casesDirectories) {
                    yield return
                        new TestCaseData(
                            provider,
                            caseDirectory.Substring(providerDirectory.Length),
                            GetSourcePath(caseDirectory),
                            GetActionFilenames(caseDirectory))
                        .SetProperty("Refactoring", provider.Provider.GetType().Name + ".AllCovered");
            }
            };
        }

        private static string GetSourceContent(string caseDirectory)
            => caseDirectory.F(GetSourcePath).F(File.ReadAllText);

        private static string GetSourcePath(string caseDirectory)
            => Path.Combine(caseDirectory, "Source.cs");

        private static string GetActionTitle(string path)
            => path.F(File.ReadLines).F(GetActionTitle);

        private static string GetActionTitle(IEnumerable<string> lines)
            => lines.First().Substring("//".Length).Trim();

        private static IEnumerable<string> GetActionFilenames(string caseDirectory) {
            return Directory
                .EnumerateFiles(caseDirectory)
                .Where(r => r != Path.Combine(caseDirectory, "Source.cs"));
        }

        private static string GetProviderDirectory(RefactoringProvider provider) {
            var dirName = provider.ToString();
            var providerDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Refactorings", dirName);
            return providerDirectory;
        }

        private static async Task ApplyActions(CustomWorkspace workspace, CodeAction action, CancellationTokenSource cts) {
            var operations = await action.GetOperationsAsync(cts.Token);
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

        private static CustomWorkspace GetWorkspace() {
            var workspace = new CustomWorkspace();

            workspace.Options = workspace.Options
                .WithChangedOption(UseTabs, CSharp, true)
                .WithChangedOption(TabSize, CSharp, 4)
                .WithChangedOption(IndentationSize, CSharp, 4)
                .WithChangedOption(NewLine, CSharp, "\r\n")
                .WithChangedOption(SmartIndent, CSharp, IndentStyle.Smart)
                .WithChangedOption(NewLinesForBracesInAnonymousMethods, false)
                .WithChangedOption(NewLinesForBracesInAnonymousTypes, false)
                .WithChangedOption(NewLinesForBracesInControlBlocks, false)
                .WithChangedOption(NewLinesForBracesInLambdaExpressionBody, false)
                .WithChangedOption(NewLinesForBracesInMethods, false)
                .WithChangedOption(NewLinesForBracesInObjectInitializers, false)
                .WithChangedOption(NewLinesForBracesInTypes, false);

            return workspace;
        }

        [Test, TestCaseSource("TestCases")]
        public async Task Test(RefactoringProvider provider, string caseName, string actionTitle, string sourcePath, string resultPath) {
            var source = sourcePath.F(File.ReadAllText);
            var resultLines = resultPath.F(File.ReadAllLines);
            var result = string.Join("\r\n", resultLines.Skip(1));

            var cursorPos = source.IndexOf(CursorSymbol);
            AreNotEqual(-1, cursorPos, "There are no cursor in test source");
            AreEqual(-1, result.IndexOf(CursorSymbol), "There are cursor in result file");

            source = source.Replace(CursorSymbol, "");

            using (var workspace = GetWorkspace()) {
                var project = workspace.AddProject("TestProject", CSharp)
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

                await provider.Provider.ComputeRefactoringsAsync(context);

                var action = actions.SingleOrDefault(r => r.Title.Trim() == actionTitle.Trim());

                IsTrue(action != null, NotFoundMessage(actions));

                await ApplyActions(workspace, action, cts);

                var newDocument = workspace.CurrentSolution.Projects.Single().Documents.Single();

                var expectedTree = SyntaxFactory.ParseCompilationUnit(result).SyntaxTree;
                var actualTree = newDocument.GetSyntaxTreeAsync(cts.Token).Result;

                IsTrue(
                    condition: SyntaxFactory.AreEquivalent(expectedTree, actualTree, false),
                    message: BadResultMessage(actualTree));
            }
        }

        [Test, TestCaseSource("ActionsTitles")]
        public void TestAllActionsCovered(RefactoringProvider provider, string caseName, string sourcePath, IEnumerable<string> actionPaths) {
            var source = File.ReadAllText(sourcePath);
            var actionTitles = actionPaths.Select(GetActionTitle);

            var cursorPos = source.IndexOf(CursorSymbol);
            AreNotEqual(-1, cursorPos, "There are no cursor in test source");
            source = source.Replace(CursorSymbol, "");

            using (var workspace = GetWorkspace()) {
                var project = workspace.AddProject("TestProject", CSharp)
                    .AddMetadataReference(corlibReference)
                    .AddMetadataReference(s_systemCoreReference);

                var document = project.AddDocument("TestCode.cs", source);

                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                var actions = new List<CodeAction>();

                var context = new CodeRefactoringContext(
                    document: document,
                    span: TextSpan.FromBounds(start: cursorPos, end: cursorPos),
                    registerRefactoring: a => { actions.Add(a); },
                    cancellationToken: cts.Token);

                provider.Provider.ComputeRefactoringsAsync(context).Wait();

                var notPresentActions = actions.Select(a => a.Title).Except(actionTitles).ToList();

                CollectionAssert.IsEmpty(notPresentActions, "Provider: {0}, Case: {1}", provider.GetType().Name, caseName);
            }
        }


        public struct RefactoringProvider {
            public CodeRefactoringProvider Provider { get; }

            public RefactoringProvider(CodeRefactoringProvider provider) {
                Provider = provider;
            }

            public override string ToString() {
                return Provider.GetType().Name.F(r => r.Substring(0, r.Length - "Provider".Length));
            }
        }
    }
}