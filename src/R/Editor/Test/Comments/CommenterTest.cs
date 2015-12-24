using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.Languages.Editor.Test.Utility;
using Microsoft.R.Editor.Comments;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class CommenterTest : UnitTestBase {
        [TestMethod]
        [TestCategory("R.Commenting")]
        public void Commenter_CommentTest01() {
            SequentialEditorTestExecutor.ExecuteTest((ManualResetEventSlim evt) => {
                string original =
    @"
    x <- 1
x <- 2
";
                ITextView textView = TextViewTest.MakeTextView(original, new TextRange(2, 0));
                ITextBuffer textBuffer = textView.TextBuffer;

                var command = new CommentCommand(textView, textBuffer);
                CommandStatus status = command.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.COMMENT_BLOCK);
                Assert.AreEqual(CommandStatus.SupportedAndEnabled, status);

                object o = null;
                command.Invoke(Guid.Empty, 0, null, ref o);

                string expected =
    @"
    #x <- 1
x <- 2
";

                string actual = textBuffer.CurrentSnapshot.GetText();
                Assert.AreEqual(expected, actual);

                evt.Set();
            });
        }

        [TestMethod]
        [TestCategory("R.Commenting")]
        public void Commenter_CommentTest02() {
            SequentialEditorTestExecutor.ExecuteTest((ManualResetEventSlim evt) => {
                string original =
@"
    x <- 1
x <- 2
";
                ITextView textView = TextViewTest.MakeTextView(original, new TextRange(8, 8));
                ITextBuffer textBuffer = textView.TextBuffer;

                var command = new CommentCommand(textView, textBuffer);
                CommandStatus status = command.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.COMMENT_BLOCK);
                Assert.AreEqual(CommandStatus.SupportedAndEnabled, status);

                object o = null;
                command.Invoke(Guid.Empty, 0, null, ref o);

                string expected =
    @"
    #x <- 1
#x <- 2
";

                string actual = textBuffer.CurrentSnapshot.GetText();
                Assert.AreEqual(expected, actual);

                evt.Set();
            });
        }

        [TestMethod]
        [TestCategory("R.Commenting")]
        public void Commenter_UncommentTest01() {
            SequentialEditorTestExecutor.ExecuteTest((ManualResetEventSlim evt) => {
                string original =
@"
    #x <- 1
x <- 2
";
                ITextView textView = TextViewTest.MakeTextView(original, new TextRange(2, 0));
                ITextBuffer textBuffer = textView.TextBuffer;

                var command = new UncommentCommand(textView, textBuffer);
                CommandStatus status = command.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK);
                Assert.AreEqual(CommandStatus.SupportedAndEnabled, status);

                object o = null;
                command.Invoke(Guid.Empty, 0, null, ref o);

                string expected =
    @"
    x <- 1
x <- 2
";

                string actual = textBuffer.CurrentSnapshot.GetText();
                Assert.AreEqual(expected, actual);

                evt.Set();
            });
        }

        [TestMethod]
        [TestCategory("R.Commenting")]
        public void Commenter_UncommentTest02() {
            SequentialEditorTestExecutor.ExecuteTest((ManualResetEventSlim evt) => {
                string original =
@"
    #x <- 1
#x <- 2
";
                ITextView textView = TextViewTest.MakeTextView(original, new TextRange(8, 8));
                ITextBuffer textBuffer = textView.TextBuffer;

                var command = new UncommentCommand(textView, textBuffer);
                CommandStatus status = command.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK);
                Assert.AreEqual(CommandStatus.SupportedAndEnabled, status);

                object o = null;
                command.Invoke(Guid.Empty, 0, null, ref o);

                string expected =
    @"
    x <- 1
x <- 2
";

                string actual = textBuffer.CurrentSnapshot.GetText();
                Assert.AreEqual(expected, actual);

                evt.Set();
            });
        }
    }
}
