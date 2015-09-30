using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test.Mocks;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Completion;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Test.Completions
{
    using Languages.Core.Text;
    using VisualStudio.Text;
    using Completion = Microsoft.VisualStudio.Language.Intellisense.Completion;

    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RCompletionSourceTest : UnitTestBase
    {
        [TestMethod]
        public void RCompletionSource_BaseFunctionsTest01()
        {
            EditorShell.SetShell(TestEditorShell.Create());

            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("", 0, completionSets);

            Assert.AreEqual(1, completionSets.Count);
            Assert.AreEqual(2315, completionSets[0].Completions.Count);

            Completion x = completionSets[0].Completions.FirstOrDefault((Completion c) => c.DisplayText == "abbreviate");
            Assert.IsNotNull(x);

            x = completionSets[0].Completions.FirstOrDefault((Completion c) => c.DisplayText == "abs");
            Assert.IsNotNull(x);
        }

        [TestMethod]
        public void RCompletionSource_BaseFunctionsTest02()
        {
            EditorShell.SetShell(TestEditorShell.Create());

            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("f", 1, completionSets, new TextRange(0, 1));

            Assert.AreEqual(1, completionSets.Count);

            completionSets[0].Filter();
            Assert.AreEqual(106, completionSets[0].Completions.Count);

            Assert.AreEqual("F", completionSets[0].Completions[0].DisplayText);
            Assert.AreEqual("Logical Vectors", completionSets[0].Completions[0].Description);

            Assert.AreEqual("factanal", completionSets[0].Completions[1].DisplayText);
            Assert.AreEqual("Factor Analysis", completionSets[0].Completions[1].Description);

            Assert.AreEqual("factor", completionSets[0].Completions[2].DisplayText);
        }

        [TestMethod]
        public void RCompletionSource_KeywordsTest01()
        {
            EditorShell.SetShell(TestEditorShell.Create());

            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("f", 1, completionSets, new TextRange(0, 1));

            Assert.AreEqual(1, completionSets.Count);

            completionSets[0].Filter();
            Assert.AreEqual(106, completionSets[0].Completions.Count);

            Completion x = completionSets[0].Completions.FirstOrDefault((Completion c) => c.DisplayText == "for");
            Assert.IsNotNull(x);
        }

        [TestMethod]
        public void RCompletionSource_PackagesTest01()
        {
            EditorShell.SetShell(TestEditorShell.Create());

            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("library(", 8, completionSets);

            Assert.AreEqual(1, completionSets.Count);

            Completion x = completionSets[0].Completions.FirstOrDefault((Completion c) => c.DisplayText == "base");
            Assert.IsNotNull(x);
            Assert.AreEqual("Base R functions.", x.Description);
        }

        [TestMethod]
        public void RCompletionSource_SpecificPackageTest01()
        {
            EditorShell.SetShell(TestEditorShell.Create());

            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("utils::", 7, completionSets);

            Assert.AreEqual(1, completionSets.Count);

            Completion x = completionSets[0].Completions.FirstOrDefault((Completion c) => c.DisplayText == "adist");
            Assert.IsNotNull(x);
            Assert.AreEqual("Approximate String Distances", x.Description);
        }

        [TestMethod]
        public void RCompletionSource_CommentsTest01()
        {
            EditorShell.SetShell(TestEditorShell.Create());

            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("#No", 3, completionSets);

            Assert.AreEqual(1, completionSets.Count);
            Assert.AreEqual(0, completionSets[0].Completions.Count);
        }

        [TestMethod]
        public void RCompletionSource_CommentsTest02()
        {
            EditorShell.SetShell(TestEditorShell.Create());

            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("#No", 0, completionSets);

            Assert.AreEqual(1, completionSets.Count);
            Assert.IsTrue(completionSets[0].Completions.Count > 0);
        }

        private void GetCompletions(string content, int position, IList<CompletionSet> completionSets, ITextRange selectedRange = null)
        {
            AstRoot ast = RParser.Parse(content);

            TextBufferMock textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            TextViewMock textView = new TextViewMock(textBuffer, position);

            if(selectedRange != null)
            {
                textView.Selection.Select(new SnapshotSpan(textBuffer.CurrentSnapshot, selectedRange.Start, selectedRange.Length), false);
            }

            CompletionSessionMock completionSession = new CompletionSessionMock(textView, completionSets, position);
            RCompletionSource completionSource = new RCompletionSource(textBuffer);

            completionSource.PopulateCompletionList(position, completionSession, completionSets, ast);
        }
    }
}
