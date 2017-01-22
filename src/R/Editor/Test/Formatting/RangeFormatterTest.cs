// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Formatting;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text.Editor;
using Xunit;

namespace Microsoft.R.Editor.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.R.Formatting]
    public class RangeFormatterTest {
        private readonly IExportProvider _exportProvider;
        private readonly IEditorShell _editorShell;

        public RangeFormatterTest(IExportProvider exportProvider) {
            _exportProvider = exportProvider;
            _editorShell = _exportProvider.GetExportedValue<IEditorShell>();
        }

        [Test]
        public void EmptyFileTest() {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView(string.Empty, out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.EmptyRange, new RFormatOptions(), _editorShell);
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            actual.Should().BeEmpty();
        }

        [Test]
        public void EmptyArgumentsTest01() {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView("c(,,)", out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.EmptyRange, new RFormatOptions(), _editorShell);
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected = "c(,,)";

            actual.Should().Be(expected);
        }

        [Test]
        public void EmptyArgumentsTest02() {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView("c[,,]", out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.EmptyRange, new RFormatOptions(), _editorShell);
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected = "c[,,]";

            actual.Should().Be(expected);
        }

        [Test]
        public void EmptyArgumentsTest03() {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView("c[[,,]]", out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.EmptyRange, new RFormatOptions(), _editorShell);
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected = "c[[,,]]";

            actual.Should().Be(expected);
        }

        [Test]
        public void ArgumentsTest01() {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView("c[[a,,]]", out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.EmptyRange, new RFormatOptions(), _editorShell);
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected = "c[[a,,]]";

            actual.Should().Be(expected);
        }

        [Test]
        public void ArgumentsTest02() {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView("c[[a,b,]]", out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.EmptyRange, new RFormatOptions(), _editorShell);
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected = "c[[a, b,]]";

            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("if(true){\nif(false){}}", 4, 7, "if (true) {\nif(false){}}")]
        [InlineData("if (a==a+((b +c) /x)){\nif(func(a,b,c +2, x =2,...)){}}", 2, 2, "if (a == a + ((b + c) / x)) {\nif(func(a,b,c +2, x =2,...)){}}")]
        public void FormatConditional(string original, int start, int end, string expected) {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView(original, out ast);
            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.FromBounds(start, end), new RFormatOptions(), _editorShell);
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatConditionalTest03() {
            AstRoot ast;
            string original =
@"if(true){
    if(false){
x<-1
    }
}";
            ITextView textView = TextViewTest.MakeTextView(original, out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, new TextRange(original.IndexOf('x'), 1), new RFormatOptions(), _editorShell);
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
        public void FormatConditionalTest04() {
            AstRoot ast;
            string original = "if (x > 1)\r\ny<-2";
            ITextView textView = TextViewTest.MakeTextView(original, out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, new TextRange(original.IndexOf('y'), 0), new RFormatOptions(), _editorShell);

            string expected = "if (x > 1)\r\n    y <- 2";
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            actual.Should().Be(expected);
        }

        [Test]
        public void FormatConditionalTest05() {
            AstRoot ast;
            string original = "if(true){\n} else {}\n";
            ITextView textView = TextViewTest.MakeTextView(original, out ast);
            RangeFormatter.FormatRange(textView, textView.TextBuffer, new TextRange(original.IndexOf("else"), 0), new RFormatOptions(), _editorShell);
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            string expected = "if(true){\n} else { }\n";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatConditionalTest06() {
            AstRoot ast;
            string original =
@"if(true)
   while(true) {
} else {}
";
            ITextView textView = TextViewTest.MakeTextView(original, out ast);
            RangeFormatter.FormatRange(textView, textView.TextBuffer, new TextRange(original.IndexOf("else"), 0), new RFormatOptions(), _editorShell);
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            string expected =
@"if(true)
   while(true) {
   } else { }
";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatOneLine() {
            AstRoot ast;
            string original =
@"foo(cache=TRUE)
foo(cache=TRUE)
";
            ITextView textView = TextViewTest.MakeTextView(original, out ast);
            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.FromBounds(0, original.LastIndexOf("foo", StringComparison.Ordinal)), new RFormatOptions(), _editorShell);
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();

            string expected =
@"foo(cache = TRUE)
foo(cache=TRUE)
";
            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("{}", 0, 1, "{ }")]
        [InlineData("{\n}", 0, 1, "{\n}")]
        [InlineData("{\n if(TRUE) {\n}}", 14, 16, "{\n if(TRUE) {\n }\n}")]
        [InlineData("{\n    {\n  } }", 6, 13, "{\n    {\n    }\n}")]
        public void FormatScope(string content, int start, int end, string expected) {
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView(content, out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.FromBounds(start, end), new RFormatOptions(), _editorShell);
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatScopeLessIf01() {
            string original =
@"
if (x != nrx) 
    stop()
    if (z < ncx)
    stop()
";
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView(original, out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, new TextRange(original.IndexOf("if (z"), 0), new RFormatOptions(), _editorShell);
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

        [Test]
        public void FormatMultiline01() {
            string original =
@"x %>%y %>%
    z %>%a";
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView(original, out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.FromBounds(
                                  original.IndexOf("z"), original.IndexOf("a") + 1), new RFormatOptions(), _editorShell);
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected =
@"x %>% y %>%
    z %>% a";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatMultiline02() {
            string original =
@"((x %>%y)
    %>% z %>%a)";
            AstRoot ast;
            ITextView textView = TextViewTest.MakeTextView(original, out ast);

            RangeFormatter.FormatRange(textView, textView.TextBuffer, TextRange.FromBounds(
                                  original.IndexOf("%>% z"), original.IndexOf("a") + 2), new RFormatOptions(), _editorShell);
            string actual = textView.TextBuffer.CurrentSnapshot.GetText();
            string expected =
@"((x %>% y)
    %>% z %>% a)";
            actual.Should().Be(expected);
        }
    }
}
