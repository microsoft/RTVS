using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SmartIndentTest : UnitTestBase {
        [TestMethod]
        [TestCategory("R.SmartIndent")]
        public void SmartIndent_NoScopeTest01() {
            int? indent = GetSmartIndent("if (x > 1)\n", 1);

            Assert.IsTrue(indent.HasValue);
            Assert.AreEqual(4, indent);
        }

        [TestMethod]
        [TestCategory("R.SmartIndent")]
        public void SmartIndent_UnclosedScopeTest01() {
            int? indent = GetSmartIndent("{if (x > 1)\r\n    x <- 1\r\nelse\n", 3);

            Assert.IsTrue(indent.HasValue);
            Assert.AreEqual(4, indent);
        }

        [TestMethod]
        [TestCategory("R.SmartIndent")]
        public void SmartIndent_UnclosedScopeTest02() {
            int? indent = GetSmartIndent("repeat\r\n    if (x > 1)\r\n", 2);

            Assert.IsTrue(indent.HasValue);
            Assert.AreEqual(8, indent);
        }

        [TestMethod]
        [TestCategory("R.SmartIndent")]
        public void SmartIndent_ScopedIfTest01() {
            int? indent = GetSmartIndent("if (x > 1) {\r\n\r\n}", 1);

            Assert.IsTrue(indent.HasValue);
            Assert.AreEqual(4, indent);
        }

        private int? GetSmartIndent(string content, int lineNumber) {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView(content, 0, out ast);
            var document = new EditorDocumentMock(new EditorTreeMock(textView.TextBuffer, ast));

            ISmartIndentProvider provider = EditorShell.Current.ExportProvider.GetExport<ISmartIndentProvider>().Value;
            ISmartIndent indenter = provider.CreateSmartIndent(textView);

            return indenter.GetDesiredIndentation(textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber));
        }
    }
}
