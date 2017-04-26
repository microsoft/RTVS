// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Formatting;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.R.Formatting]
    public class RangeFormatterTest {
        private readonly IServiceContainer _services;
        private readonly IREditorSettings _settings;

        public RangeFormatterTest(IServiceContainer services) {
            _services = services;
            _settings = services.GetService<IREditorSettings>();
        }

        [Test]
        public void EmptyFileTest() {
            var editorView = TextViewTest.MakeTextView(string.Empty, out AstRoot ast);
            RangeFormatter.FormatRange(editorView, editorView.EditorBuffer, TextRange.EmptyRange, _settings, _services);

            var actual = editorView.EditorBuffer.CurrentSnapshot.GetText();
            actual.Should().BeEmpty();
        }

        [CompositeTest]
        [InlineData("c(,,)", "c(,,)")]
        [InlineData("c[,,]", "c[,,]")]
        [InlineData("c[[,,]]", "c[[,,]]")]
        [InlineData("c[[a,,]]", "c[[a,,]]")]
        [InlineData("c[[a,b,]]", "c[[a, b,]]")]
        public void FormatArgumentsTest(string content, string expected) {
            var editorView = TextViewTest.MakeTextView(content, out AstRoot ast);
            RangeFormatter.FormatRange(editorView, editorView.EditorBuffer, TextRange.EmptyRange, _settings, _services);

            var actual = editorView.EditorBuffer.CurrentSnapshot.GetText();
            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("if(true){\nif(false){}}", 4, 7, "if (true) {\nif(false){}}")]
        [InlineData("if (a==a+((b +c) /x)){\nif(func(a,b,c +2, x =2,...)){}}", 2, 2, "if (a == a + ((b + c) / x)) {\nif(func(a,b,c +2, x =2,...)){}}")]
        public void FormatConditional(string original, int start, int end, string expected) {
            var editorView = TextViewTest.MakeTextView(original, out AstRoot ast);
            RangeFormatter.FormatRange(editorView, editorView.EditorBuffer, TextRange.FromBounds(start, end), _settings, _services);
            string actual = editorView.EditorBuffer.CurrentSnapshot.GetText();
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatConditionalTest03() {
            var original =
@"if(true){
  if(false){
x<-1
  }
}";
            var editorView = TextViewTest.MakeTextView(original, out AstRoot ast);

            RangeFormatter.FormatRange(editorView, editorView.EditorBuffer, new TextRange(original.IndexOf('x'), 1), _settings, _services);
            var actual = editorView.EditorBuffer.CurrentSnapshot.GetText();

            var expected =
@"if(true){
  if(false){
    x <- 1
  }
}";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatConditionalTest04() {
            var original = "if (x > 1)\r\ny<-2";
            var editorView = TextViewTest.MakeTextView(original, out AstRoot ast);

            RangeFormatter.FormatRange(editorView, editorView.EditorBuffer, new TextRange(original.IndexOf('y'), 0), _settings, _services);

            var expected = "if (x > 1)\r\n  y <- 2";
            var actual = editorView.EditorBuffer.CurrentSnapshot.GetText();

            actual.Should().Be(expected);
        }

        [Test]
        public void FormatConditionalTest05() {
            var original = "if(true){\n} else {}\n";
            var editorView = TextViewTest.MakeTextView(original, out AstRoot ast);
            RangeFormatter.FormatRange(editorView, editorView.EditorBuffer, new TextRange(original.IndexOf("else"), 0), _settings, _services);
            var actual = editorView.EditorBuffer.CurrentSnapshot.GetText();

            var expected = "if(true){\n} else { }\n";
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
            var editorView = TextViewTest.MakeTextView(original, out ast);
            RangeFormatter.FormatRange(editorView, editorView.EditorBuffer, new TextRange(original.IndexOf("else"), 0), _settings, _services);
            string actual = editorView.EditorBuffer.CurrentSnapshot.GetText();

            string expected =
@"if(true)
   while(true) {
   } else { }
";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatOneLine() {
            var original =
@"foo(cache=TRUE)
foo(cache=TRUE)
";
            var editorView = TextViewTest.MakeTextView(original, out AstRoot ast);
            RangeFormatter.FormatRange(editorView, editorView.EditorBuffer, TextRange.FromBounds(0, original.LastIndexOf("foo", StringComparison.Ordinal)), _settings, _services);
            var actual = editorView.EditorBuffer.CurrentSnapshot.GetText();

            var expected =
@"foo(cache = TRUE)
foo(cache=TRUE)
";
            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("{}", 0, 1, "{ }")]
        [InlineData("{\n}", 0, 1, "{\n}")]
        [InlineData("{\n if(TRUE) {\n}}", 14, 16, "{\n if(TRUE) {\n }\n}")]
        [InlineData("{\n    {\n  } }", 6, 13, "{\n  {\n  }\n}")]
        public void FormatScope(string content, int start, int end, string expected) {
            var editorView = TextViewTest.MakeTextView(content, out AstRoot ast);
            RangeFormatter.FormatRange(editorView, editorView.EditorBuffer, TextRange.FromBounds(start, end), _settings, _services);
            var actual = editorView.EditorBuffer.CurrentSnapshot.GetText();
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatScopeLessIf01() {
            var original =
@"
if (x != nrx) 
    stop()
    if (z < ncx)
    stop()
";
            var editorView = TextViewTest.MakeTextView(original, out AstRoot ast);
            RangeFormatter.FormatRange(editorView, editorView.EditorBuffer, new TextRange(original.IndexOf("if (z"), 0), _settings, _services);
            var actual = editorView.EditorBuffer.CurrentSnapshot.GetText();
            var expected =
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
            var original =
@"x %>%y %>%
    z %>%a";
            var editorView = TextViewTest.MakeTextView(original, out AstRoot ast);

            RangeFormatter.FormatRange(editorView, editorView.EditorBuffer, TextRange.FromBounds(
                                  original.IndexOf("z"), original.IndexOf("a") + 1), _settings, _services);
            var actual = editorView.EditorBuffer.CurrentSnapshot.GetText();
            var expected =
@"x %>% y %>%
    z %>% a";
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatMultiline02() {
            var original =
@"((x %>%y)
    %>% z %>%a)";
            var editorView = TextViewTest.MakeTextView(original, out AstRoot ast);

            RangeFormatter.FormatRange(editorView, editorView.EditorBuffer, TextRange.FromBounds(
                                  original.IndexOf("%>% z"), original.IndexOf("a") + 2), _settings, _services);
            var actual = editorView.EditorBuffer.CurrentSnapshot.GetText();
            var expected =
@"((x %>% y)
    %>% z %>% a)";
            actual.Should().Be(expected);
        }
    }
}
