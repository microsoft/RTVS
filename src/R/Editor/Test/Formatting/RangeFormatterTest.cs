using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Formatting;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.R.Formatting]
    public class RangeFormatterTest {
        [Test]
        public void RangeFormatter_EmptyFileTest() {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView(string.Empty, out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.EmptyRange, ast, new RFormatOptions());
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            actual.Should().BeEmpty();
        }

        [Test]
        public void RangeFormatter_EmptyArgumentsTest01() {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView("c(,,)", out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.EmptyRange, ast, new RFormatOptions());
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected = "c(,,)";

            actual.Should().Be(expected);
        }

        [Test]
        public void RangeFormatter_EmptyArgumentsTest02() {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView("c[,,]", out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.EmptyRange, ast, new RFormatOptions());
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected = "c[,,]";

            actual.Should().Be(expected);
        }

        [Test]
        public void RangeFormatter_EmptyArgumentsTest03() {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView("c[[,,]]", out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.EmptyRange, ast, new RFormatOptions());
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected = "c[[,,]]";

            actual.Should().Be(expected);
        }

        [Test]
        public void RangeFormatter_ArgumentsTest01() {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView("c[[a,,]]", out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.EmptyRange, ast, new RFormatOptions());
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected = "c[[a,,]]";

            actual.Should().Be(expected);
        }

        [Test]
        public void RangeFormatter_ArgumentsTest02() {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView("c[[a,b,]]", out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.EmptyRange, ast, new RFormatOptions());
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected = "c[[a, b,]]";

            actual.Should().Be(expected);
        }

        [Test]
        public void RangeFormatter_FormatConditionalTest01() {
            AstRoot ast;
            string original = "if(true){if(false){}}";
            ITextView textView = TextViewTest.MakeTextView(original, out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, new TextRange(4, 3), ast, new RFormatOptions());
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            string expected =
@"if (true) {
  if (false) { }
}";

            actual.Should().Be(expected);
        }

        [Test]
        public void RangeFormatter_FormatConditionalTest02() {
            AstRoot ast;
            string original =
@"if (a==a+((b +c) /x)){ 
if(func(a,b,c +2, x =2,...)){}}";

            ITextView textView = TextViewTest.MakeTextView(original, out ast);
            RangeFormatter.FormatRange(textView, textView.TextBuffer, new TextRange(2, 0), ast, new RFormatOptions());

            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected =
@"if (a == a + ((b + c) / x)) {
if(func(a,b,c +2, x =2,...)){}}";

            actual.Should().Be(expected);
        }

        [Test]
        public void RangeFormatter_FormatConditionalTest03() {
            AstRoot ast;
            string original =
@"if(true){
    if(false){
x<-1
    }
}";
            ITextView textView = TextViewTest.MakeTextView(original, out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, new TextRange(original.IndexOf('x'), 1), ast, new RFormatOptions());
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            string expected =
@"if(true){
    if(false){
        x <- 1
    }
}";
            actual.Should().Be(expected);
        }

        [Test]
        public void RangeFormatter_FormatConditionalTest04() {
            AstRoot ast;
            string original = "if (x > 1)\r\ny<-2";
            ITextView textView = TextViewTest.MakeTextView(original, out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, new TextRange(original.IndexOf('y'), 0), ast, new RFormatOptions());

            string expected = "if (x > 1)\r\n    y <- 2";
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            actual.Should().Be(expected);
        }


        [Test]
        public void RangeFormatter_FormatOneLine() {
            AstRoot ast;
            string original =
@"foo(cache=TRUE)
foo(cache=TRUE)
";
            ITextView textView = TextViewTest.MakeTextView(original, out ast);
            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.FromBounds(0, original.LastIndexOf("foo", StringComparison.Ordinal)), ast, new RFormatOptions());
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            string expected =
@"foo(cache = TRUE)
foo(cache=TRUE)
";
            actual.Should().Be(expected);
        }

        [Test]
        public void RangeFormatter_FormatSimpleScope() {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView("{}", out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.FromBounds(0, 1), ast, new RFormatOptions());
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            actual.Should().Be("{ }");
        }

        [Test]
        public void RangeFormatter_FormatScopeLessIf01() {
            string original =
@"
if (x != nrx) 
    stop()
    if (z < ncx)
    stop()
";
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView(original, out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, new TextRange(original.IndexOf("if (z"), 0), ast, new RFormatOptions());
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected =
@"
if (x != nrx) 
    stop()
if (z < ncx)
    stop()
";

            actual.Should().Be(expected);
        }
    }
}
