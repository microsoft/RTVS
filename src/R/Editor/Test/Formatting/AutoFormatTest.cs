using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Formatting;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.SmartIndent;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Test.Formatting
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class AutoFormatTest : UnitTestBase
    {
        [TestMethod]
        public void AutoFormat_TypeOneLineTest()
        {
            ITextView textView = TestAutoFormat(0, "x<-1\n");
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            Assert.AreEqual("x <- 1\n", actual);
            Assert.AreEqual(7, textView.Caret.Position.BufferPosition);
        }

        [TestMethod]
        public void AutoFormat_SmartIndentTest01()
        {
            ITextView textView = TestAutoFormat(8, "\n", "if(x>1){}");

            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected = "if (x > 1) {\n    \r\n}";

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(17, textView.Caret.Position.BufferPosition);
        }

        [TestMethod]
        public void AutoFormat_SmartIndentTest02()
        {
            ITextView textView = TestAutoFormat(12, "\n", "if (x > 1) {\r\n    \r\n}");

            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected = "if (x > 1) {\n\r\n    \r\n}";

            Assert.AreEqual(expected, actual);
            // 12 if off by one (should be 13 in real life) due to limitation
            // of the mocked text view, text buffer and caret position tracking
            Assert.AreEqual(12, textView.Caret.Position.BufferPosition);
        }

        [TestMethod]
        public void AutoFormat_SmartIndentTest03()
        {
            ITextView textView = TestAutoFormat(14, "\n", "if (x > 1) {\r\n}");

            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected = "if (x > 1) {\r\n\n}";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AutoFormat_SmartIndentTest04()
        {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView("if (x > 1) {\r\n\r\n}", 0, out ast);
            var document = new EditorDocumentMock(new EditorTreeMock(textView.TextBuffer, ast));

            ISmartIndentProvider provider = EditorShell.ExportProvider.GetExport<ISmartIndentProvider>().Value;
            ISmartIndent indenter = provider.CreateSmartIndent(textView);
            int? indent = indenter.GetDesiredIndentation(textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1));

            Assert.IsTrue(indent.HasValue);
            Assert.AreEqual(4, indent);
        }

        [TestMethod]
        public void AutoFormat_SmartIndentTest05()
        {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView("  x <- 1\r\n", 0, out ast);
            var document = new EditorDocumentMock(new EditorTreeMock(textView.TextBuffer, ast));

            ISmartIndentProvider provider = EditorShell.ExportProvider.GetExport<ISmartIndentProvider>().Value;
            SmartIndenter indenter = (SmartIndenter)provider.CreateSmartIndent(textView);

            int? indent = indenter.GetDesiredIndentation(textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1), IndentStyle.Block);

            Assert.IsTrue(indent.HasValue);
            Assert.AreEqual(2, indent);
        }

        private ITextView TestAutoFormat(int position, string textToType, string initialContent = "")
        {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView(initialContent, position, out ast);

            textView.TextBuffer.Changed += (object sender, TextContentChangedEventArgs e) =>
            {
                if (e.Changes[0].NewText.Length == 1)
                {
                    if (e.Changes[0].NewText[0] == '\r' || e.Changes[0].NewText[0] == '\n')
                    {
                        ITextSnapshotLine line = e.Before.GetLineFromPosition(position);
                        position = line.Length + 1;
                        textView.Caret.MoveTo(new SnapshotPoint(e.After, position));
                    }

                    AutoFormat.HandleAutoFormat(textView, textView.TextBuffer, ast, e.Changes[0].NewText[0]);
                }
                else
                {
                    ITextSnapshotLine line = e.After.GetLineFromPosition(position);
                    textView.Caret.MoveTo(new SnapshotPoint(e.After, line.Length + 1));
                }
            };

            Typing.Type(textView.TextBuffer, position, textToType);

            return textView;
        }
    }
}
