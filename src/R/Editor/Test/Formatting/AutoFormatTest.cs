using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Formatting;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.SmartIndent;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class AutoFormatTest : UnitTestBase {
        [TestMethod]
        [TestCategory("R.Autoformat")]
        public void AutoFormat_TypeOneLineTest() {
            ITextView textView = TestAutoFormat(0, "x<-1\n");
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            Assert.AreEqual("x <- 1\n", actual);
            Assert.AreEqual(7, textView.Caret.Position.BufferPosition);
        }

        [TestMethod]
        [TestCategory("R.Autoformat")]
        public void AutoFormat_FunctionDefinitionTest01() {
            ITextView textView = TestAutoFormat(16, "\n", "x<-function(x,y,");

            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected = "x <- function(x, y,\n";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("R.Autoformat")]
        public void AutoFormat_SmartIndentTest05() {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView("  x <- 1\r\n", 0, out ast);
            var document = new EditorDocumentMock(new EditorTreeMock(textView.TextBuffer, ast));

            ISmartIndentProvider provider = EditorShell.Current.ExportProvider.GetExport<ISmartIndentProvider>().Value;
            SmartIndenter indenter = (SmartIndenter)provider.CreateSmartIndent(textView);

            int? indent = indenter.GetDesiredIndentation(textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1), IndentStyle.Block);

            Assert.IsTrue(indent.HasValue);
            Assert.AreEqual(2, indent);
        }

        private ITextView TestAutoFormat(int position, string textToType, string initialContent = "") {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView(initialContent, position, out ast);

            textView.TextBuffer.Changed += (object sender, TextContentChangedEventArgs e) => {
                List<TextChangeEventArgs> textChanges = TextUtility.ConvertToRelative(e);
                ast.ReflectTextChanges(textChanges);

                if (e.Changes[0].NewText.Length == 1) {
                    char ch = e.Changes[0].NewText[0];
                    if (AutoFormat.IsAutoformatTriggerCharacter(ch)) {
                        int offset = 0;
                        if (e.Changes[0].NewText[0] == '\r' || e.Changes[0].NewText[0] == '\n') {
                            position = e.Changes[0].OldPosition + 1;
                            textView.Caret.MoveTo(new SnapshotPoint(e.After, position));
                            offset = -1;
                        }
                        FormatOperations.FormatLine(textView, textView.TextBuffer, ast, offset);
                    }
                } else {
                    ITextSnapshotLine line = e.After.GetLineFromPosition(position);
                    textView.Caret.MoveTo(new SnapshotPoint(e.After, Math.Min(e.After.Length, line.Length + 1)));
                }
            };

            Typing.Type(textView.TextBuffer, position, textToType);

            return textView;
        }
    }
}
