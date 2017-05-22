// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Formatting;
using Microsoft.R.Editor.Formatting.Data;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Xunit;

namespace Microsoft.R.Editor.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.R.Formatting]
    public class FormatCommandTest {
        private readonly IServiceContainer _services;

        public FormatCommandTest(IServiceContainer services) {
            _services = services;
        }

        [CompositeTest]
        [InlineData("if(x<1){x<-2}", "if (x < 1) { x <- 2 }")]
        [InlineData("if(x<1){\nx<-2}", "if (x < 1) {\n    x <- 2\n}")]
        [InlineData("\r\nif(x<1){x<-2}", "\r\nif (x < 1) { x <- 2 }")]
        [InlineData("\r\nif(x<1){x<-2\n}", "\r\nif (x < 1) {\r\n    x <- 2\r\n}")]
        public void FormatDocument(string original, string expected) {
            var textBuffer = new TextBufferMock(original, RContentTypeDefinition.ContentType);
            var textView = new TextViewMock(textBuffer);

            using (var command = new FormatDocumentCommand(textView, textBuffer, _services)) {
                var status = command.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.FORMATDOCUMENT);
                status.Should().Be(CommandStatus.SupportedAndEnabled);

                object o = new object();
                command.Invoke(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.FORMATDOCUMENT, null, ref o);
            }

            var actual = textBuffer.CurrentSnapshot.GetText();
            actual.Should().Be(expected);
        }

        [Test]
        public void FormatOnPasteStatus() {
            var textBuffer = new TextBufferMock(string.Empty, RContentTypeDefinition.ContentType);
            var textView = new TextViewMock(textBuffer);
            var clipboard = new ClipboardDataProvider();

            using (var command = new FormatOnPasteCommand(textView, textBuffer, _services)) {
                command.ClipboardDataProvider = clipboard;

                var status = command.Status(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Paste);
                status.Should().Be(CommandStatus.NotSupported);

                clipboard.Format = DataFormats.UnicodeText;
                clipboard.Data = "data";

                status = command.Status(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Paste);
                status.Should().Be(CommandStatus.SupportedAndEnabled);
            }
        }

        [CompositeTest]
        [InlineData("if(x<1){x<-2}", "if (x < 1) { x <- 2 }")]
        [InlineData("\"a\r\nb\r\nc\"", "\"a\r\nb\r\nc\"")]
        public void FormatOnPaste(string content, string expected) {
            var actual = FormatFromClipboard(content);
            actual.Should().Be(expected);
        }

        private string FormatFromClipboard(string content) {
            var textBuffer = new TextBufferMock(string.Empty, RContentTypeDefinition.ContentType);
            var editorBuffer = textBuffer.ToEditorBuffer();
            var textView = new TextViewMock(textBuffer);
            var clipboard = new ClipboardDataProvider();

            using (var command = new FormatOnPasteCommand(textView, textBuffer, _services)) {
                command.ClipboardDataProvider = clipboard;

                clipboard.Format = DataFormats.UnicodeText;
                clipboard.Data = content;

                var ast = RParser.Parse(textBuffer.CurrentSnapshot.GetText());
                using (var document = new EditorDocumentMock(new EditorTreeMock(editorBuffer, ast))) {
                    var o = new object();
                    command.Invoke(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Paste, null, ref o);
                }
            }

            return textBuffer.CurrentSnapshot.GetText();
        }

        private class ClipboardDataProvider : IClipboardDataProvider {
            public string Format { get; set; }
            public object Data { get; set; }

            public bool ContainsData(string format) => format == Format;
            public object GetData(string format) => format == Format ? Data : null;
        }
    }
}
