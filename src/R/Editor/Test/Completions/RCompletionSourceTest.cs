using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Completion;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Test.Completions {
    using Languages.Core.Text;
    using VisualStudio.Editor.Mocks;
    using VisualStudio.Text;
    using Completion = Microsoft.VisualStudio.Language.Intellisense.Completion;

    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RCompletionSourceTest : UnitTestBase {
        [TestMethod]
        [TestCategory("R.Completion")]
        public void RCompletionSource_BaseFunctionsTest01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("", 0, completionSets);

            Assert.AreEqual(1, completionSets.Count);

            Completion x = completionSets[0].Completions.FirstOrDefault((Completion c) => c.DisplayText == "abbreviate");
            Assert.IsNotNull(x);

            x = completionSets[0].Completions.FirstOrDefault((Completion c) => c.DisplayText == "abs");
            Assert.IsNotNull(x);
        }

        [TestMethod]
        [TestCategory("R.Completion")]
        public void RCompletionSource_BaseFunctionsTest02() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("f", 1, completionSets, new TextRange(0, 1));

            Assert.AreEqual(1, completionSets.Count);
            completionSets[0].Filter();

            Assert.AreEqual("factanal", completionSets[0].Completions[0].DisplayText);
            Assert.AreEqual("Factors", completionSets[0].Completions[1].Description);
        }

        [TestMethod]
        [TestCategory("R.Completion")]
        public void RCompletionSource_KeywordsTest01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("f", 1, completionSets, new TextRange(0, 1));

            Assert.AreEqual(1, completionSets.Count);

            completionSets[0].Filter();
            Completion x = completionSets[0].Completions.FirstOrDefault((Completion c) => c.DisplayText == "for");
            Assert.IsNotNull(x);
        }

        [TestMethod]
        [TestCategory("R.Completion")]
        public void RCompletionSource_PackagesTest01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("library(", 8, completionSets);

            Assert.AreEqual(1, completionSets.Count);

            Completion x = completionSets[0].Completions.FirstOrDefault((Completion c) => c.DisplayText == "base");
            Assert.IsNotNull(x);
            Assert.AreEqual("Base R functions.", x.Description);
        }

        [TestMethod]
        [TestCategory("R.Completion")]
        public void RCompletionSource_SpecificPackageTest01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("utils::", 7, completionSets);

            Assert.AreEqual(1, completionSets.Count);

            Completion x = completionSets[0].Completions.FirstOrDefault((Completion c) => c.DisplayText == "adist");
            Assert.IsNotNull(x);
            Assert.AreEqual("Approximate String Distances", x.Description);
        }

        [TestMethod]
        [TestCategory("R.Completion")]
        public void RCompletionSource_CommentsTest01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("#No", 3, completionSets);

            Assert.AreEqual(1, completionSets.Count);
            Assert.AreEqual(0, completionSets[0].Completions.Count);
        }

        [TestMethod]
        [TestCategory("R.Completion")]
        public void RCompletionSource_CommentsTest02() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("#No", 0, completionSets);

            Assert.AreEqual(1, completionSets.Count);
            Assert.IsTrue(completionSets[0].Completions.Count > 0);
        }

        [TestMethod]
        [TestCategory("R.Completion")]
        public void RCompletionSource_FunctionDefinitionTest01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("x <- function()", 14, completionSets);

            Assert.AreEqual(1, completionSets.Count);
            Assert.AreEqual(0, completionSets[0].Completions.Count);
        }

        [TestMethod]
        [TestCategory("R.Completion")]
        public void RCompletionSource_FunctionDefinitionTest02() {
            for (int i = 14; i <= 18; i++) {
                List<CompletionSet> completionSets = new List<CompletionSet>();
                GetCompletions("x <- function(a, b)", i, completionSets);

                Assert.AreEqual(1, completionSets.Count);
                Assert.AreEqual(0, completionSets[0].Completions.Count);
            }
        }

        [TestMethod]
        [TestCategory("R.Completion")]
        public void RCompletionSource_FunctionDefinitionTest03() {
            for (int i = 14; i <= 19; i++) {
                List<CompletionSet> completionSets = new List<CompletionSet>();
                GetCompletions("x <- function(a, b = x+y)", i, completionSets);

                Assert.AreEqual(1, completionSets.Count);
                Assert.AreEqual(0, completionSets[0].Completions.Count);
            }

            for (int i = 20; i <= 24; i++) {
                List<CompletionSet> completionSets = new List<CompletionSet>();
                GetCompletions("x <- function(a, b = x+y)", i, completionSets);

                Assert.AreNotEqual(0, completionSets.Count);
                Assert.AreNotEqual(0, completionSets[0].Completions.Count);
            }
        }

        [TestMethod]
        [TestCategory("R.Completion")]
        public void RCompletionSource_CaseSentivityTest() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("x <- T", 6, completionSets);

            Assert.AreEqual(1, completionSets.Count);

            completionSets[0].Filter();
            Assert.AreNotEqual(0, completionSets[0].Completions.Count);

            for (int i = 0; i < completionSets[0].Completions.Count; i++) {
                Assert.AreEqual('T', completionSets[0].Completions[i].DisplayText[0]);
            }
        }

        private void GetCompletions(string content, int position, IList<CompletionSet> completionSets, ITextRange selectedRange = null) {
            AstRoot ast = RParser.Parse(content);

            TextBufferMock textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            TextViewMock textView = new TextViewMock(textBuffer, position);

            if (selectedRange != null) {
                textView.Selection.Select(new SnapshotSpan(textBuffer.CurrentSnapshot, selectedRange.Start, selectedRange.Length), false);
            }

            CompletionSessionMock completionSession = new CompletionSessionMock(textView, completionSets, position);
            RCompletionSource completionSource = new RCompletionSource(textBuffer);

            completionSource.PopulateCompletionList(position, completionSession, completionSets, ast);
        }
    }
}
