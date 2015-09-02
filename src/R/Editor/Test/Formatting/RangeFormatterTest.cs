using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test.Mocks;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
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
            ITextView textView = MakeTextView(string.Empty);

            RangeFormatter.FormatSpan(textView, new Span(0, 0), new RFormatOptions());
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void RangeFormatter_FormatConditionalTest01()
        {
            string original = "if(true){if(false){}}";
            ITextView textView = MakeTextView(original);

            RangeFormatter.FormatSpan(textView, new Span(4, 3), new RFormatOptions());
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
            string original =
@"if (a==a+((b +c) /x)){ 
if(func(a,b,c +2, x =2,...)){}}";

            ITextView textView = MakeTextView(original);
            RangeFormatter.FormatSpan(textView, new Span(2,0), new RFormatOptions());

            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected =
@"if (a == a + ((b + c) / x)) {
if(func(a,b,c +2, x =2,...)){}}";

            Assert.AreEqual(expected, actual);
        }

        private ITextView MakeTextView(string content)
        {
            EditorShell.SetShell(TestEditorShell.Create());

            TextBufferMock textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            return new TextViewMock(textBuffer);
        }
    }
}
