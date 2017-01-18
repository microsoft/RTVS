// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.Controller;
using Microsoft.R.Editor.Comments;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Test.Comments {
    [ExcludeFromCodeCoverage]
    [Category.R.Commenting]
    public class CommenterTest {
        private readonly IExportProvider _exportProvider;
        private readonly IEditorShell _editorShell;

        public CommenterTest(IExportProvider exportProvider) {
            _exportProvider = exportProvider;
            _editorShell = _exportProvider.GetExportedValue<IEditorShell>();
        }

        [Test]
        public void Commenter_CommentTest01() {
            string original =
@"
    x <- 1
x <- 2
";
            ITextView textView = TextViewTest.MakeTextView(original, new TextRange(2, 0));
            ITextBuffer textBuffer = textView.TextBuffer;

            var command = new CommentCommand(textView, textBuffer, _editorShell);
            CommandStatus status = command.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.COMMENT_BLOCK);
            status.Should().Be(CommandStatus.SupportedAndEnabled);


            object o = null;
            command.Invoke(Guid.Empty, 0, null, ref o);

            string expected =
@"
    #x <- 1
x <- 2
";

            string actual = textBuffer.CurrentSnapshot.GetText();
            actual.Should().Be(expected);
        }

        [Test]
        public void Commenter_CommentTest02() {
             string original =
@"
    x <- 1
x <- 2
";
            ITextView textView = TextViewTest.MakeTextView(original, new TextRange(8, 8));
            ITextBuffer textBuffer = textView.TextBuffer;

            var command = new CommentCommand(textView, textBuffer, _editorShell);
            CommandStatus status = command.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.COMMENT_BLOCK);
            status.Should().Be(CommandStatus.SupportedAndEnabled);

            object o = null;
            command.Invoke(Guid.Empty, 0, null, ref o);

            string expected =
    @"
    #x <- 1
#x <- 2
";

            string actual = textBuffer.CurrentSnapshot.GetText();
            actual.Should().Be(expected);
        }

        [Test]
        public void Commenter_UncommentTest01() {
            string original =
@"
    #x <- 1
x <- 2
";
            ITextView textView = TextViewTest.MakeTextView(original, new TextRange(2, 0));
            ITextBuffer textBuffer = textView.TextBuffer;

            var command = new UncommentCommand(textView, textBuffer, _editorShell);
            CommandStatus status = command.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK);
            status.Should().Be(CommandStatus.SupportedAndEnabled);

            object o = null;
            command.Invoke(Guid.Empty, 0, null, ref o);

            string expected =
@"
    x <- 1
x <- 2
";

            string actual = textBuffer.CurrentSnapshot.GetText();
            actual.Should().Be(expected);
        }

        [Test]
        public void Commenter_UncommentTest02() {
            string original =
@"
#x <- 1
#x <- 2
";
            ITextView textView = TextViewTest.MakeTextView(original, new TextRange(8, 8));
            ITextBuffer textBuffer = textView.TextBuffer;

            var command = new UncommentCommand(textView, textBuffer, _editorShell);
            CommandStatus status = command.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK);
            status.Should().Be(CommandStatus.SupportedAndEnabled);

            object o = null;
            command.Invoke(Guid.Empty, 0, null, ref o);

            string expected =
@"
x <- 1
x <- 2
";

            string actual = textBuffer.CurrentSnapshot.GetText();
            actual.Should().Be(expected);
        }
    }
}
