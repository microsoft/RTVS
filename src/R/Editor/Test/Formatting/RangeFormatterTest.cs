using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test.Mocks;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Test.Formatting
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RangeFormatterTest : UnitTestBase
    {
        [TestMethod]
        public void RangeFormatter_EmptyFileTest()
        {
            AstRoot ast;
            ITextView textView = MakeTextView(string.Empty, out ast);

            RangeFormatter.FormatRange(textView, TextRange.EmptyRange, ast, new RFormatOptions());
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void RangeFormatter_FormatConditionalTest01()
        {
            AstRoot ast;
            string original = "if(true){if(false){}}";
            ITextView textView = MakeTextView(original, out ast);

            RangeFormatter.FormatRange(textView, new TextRange(4, 3), ast, new RFormatOptions());
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            string expected =
@"if (true) {
    if (false) {
    }
}";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RangeFormatter_FormatConditionalTest02()
        {
            AstRoot ast;
            string original =
@"if (a==a+((b +c) /x)){ 
if(func(a,b,c +2, x =2,...)){}}";

            ITextView textView = MakeTextView(original, out ast);
            RangeFormatter.FormatRange(textView, new TextRange(2,0), ast, new RFormatOptions());

            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected =
@"if (a == a + ((b + c) / x)) {
if(func(a,b,c +2, x =2,...)){}}";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RangeFormatter_FormatConditionalTest03()
        {
            AstRoot ast;
            string original =
@"if(true){
    if(false){
x<-1
    }
}";
            ITextView textView = MakeTextView(original, out ast);

            RangeFormatter.FormatRange(textView, new TextRange(original.IndexOf('x'), 1), ast, new RFormatOptions());
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            string expected =
@"if(true){
    if(false){
        x <- 1
    }
}";
            Assert.AreEqual(expected, actual);
        }

        private ITextView MakeTextView(string content, out AstRoot ast)
        {
            EditorShell.SetShell(TestEditorShell.Create());

            ast = RParser.Parse(content);
            TextBufferMock textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            return new TextViewMock(textBuffer);
        }
    }
}
