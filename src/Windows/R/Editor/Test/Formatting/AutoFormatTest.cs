// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Test.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Formatting;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Xunit;

namespace Microsoft.R.Editor.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.R.Formatting]
    public class AutoFormatTest {
        private readonly IServiceContainer _services;

        public AutoFormatTest(IServiceContainer services) {
            _services = services;
        }

        [CompositeTest]
        [InlineData("x<-function(x,y,", 16, "x <- function(x, y,\n")]
        [InlineData("'x<-1'", 5, "'x<-1\n'")]
        [InlineData("x<-1", 4, "x <- 1\n")]
        [InlineData("x(a,b,c,d)", 6, "x(a, b,\nc,d)")]
        [InlineData("x(a,b,    c, d)", 8, "x(a, b,\n  c, d)")]
        public void FormatTest(string content, int position, string expected) {
            var textView = TestAutoFormat(position, content);

            var actual = textView.TextBuffer.CurrentSnapshot.GetText();
            actual.Should().Be(expected);
        }

        [Test]
        public void SmartIndentTest05() {
            var editorView = TextViewTest.MakeTextView("  x <- 1\r\n", 0, out AstRoot ast);
            using (var document = new EditorDocumentMock(new EditorTreeMock(editorView.EditorBuffer, ast))) {
                var locator = _services.GetService<IContentTypeServiceLocator>();
                var provider = locator.GetService<ISmartIndentProvider>(RContentTypeDefinition.ContentType);
                var tv = editorView.As<ITextView>();

                var settings = _services.GetService<IWritableREditorSettings>();
                settings.IndentStyle = IndentStyle.Block;

                var indenter = provider.CreateSmartIndent(tv);
                var indent = indenter.GetDesiredIndentation(tv.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(1));
                indent.Should().HaveValue().And.Be(2);
            }
        }

        private ITextView TestAutoFormat(int position, string initialContent = "") {
            var editorView = TextViewTest.MakeTextView(initialContent, position, out AstRoot ast);
            var textView = editorView.As<ITextView>();
            var af = new AutoFormat(textView, _services);
            textView.TextBuffer.Changed += (s, e) => {
                var tc = e.ToTextChange();
                ast.ReflectTextChange(tc.Start, tc.OldLength, tc.NewLength, tc.NewTextProvider);

                if (e.Changes[0].NewText.Length == 1) {
                    var ch = e.Changes[0].NewText[0];
                    if (af.IsPostProcessAutoformatTriggerCharacter(ch)) {
                        position = e.Changes[0].OldPosition + 1;
                        textView.Caret.MoveTo(new SnapshotPoint(e.After, position));
                        FormatOperations.FormatViewLine(editorView, editorView.EditorBuffer, -1, _services);
                    }
                } else {
                    var line = e.After.GetLineFromPosition(position);
                    textView.Caret.MoveTo(new SnapshotPoint(e.After, Math.Min(e.After.Length, line.Length + 1)));
                }
            };

            Typing.Type(textView.TextBuffer, position, "\n");
            return textView;
        }
    }
}
