// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Editor.Comments;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Test.Comments {
    [ExcludeFromCodeCoverage]
    [Category.R.Commenting]
    public class CommenterTest {
        private readonly IServiceContainer _services;

        public CommenterTest(IServiceContainer services) {
            _services = services;
        }

        [Test]
        public void Commenter_CommentTest01() {
            string original =
@"
    x <- 1
x <- 2
";
            var textView = TextViewTest.MakeTextView(original, new TextRange(2, 0)).As<ITextView>();
            var textBuffer = textView.TextBuffer;

            var command = new CommentCommand(textView, textBuffer, _services);
            var status = command.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.COMMENT_BLOCK);
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
            var textView = TextViewTest.MakeTextView(original, new TextRange(8, 8)).As<ITextView>();
            var textBuffer = textView.TextBuffer;

            var command = new CommentCommand(textView, textBuffer, _services);
            var status = command.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.COMMENT_BLOCK);
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
            ITextView textView = TextViewTest.MakeTextView(original, new TextRange(2, 0)).As<ITextView>();
            ITextBuffer textBuffer = textView.TextBuffer;

            var command = new UncommentCommand(textView, textBuffer, _services);
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
            ITextView textView = TextViewTest.MakeTextView(original, new TextRange(8, 8)).As<ITextView>();
            ITextBuffer textBuffer = textView.TextBuffer;

            var command = new UncommentCommand(textView, textBuffer, _services);
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
