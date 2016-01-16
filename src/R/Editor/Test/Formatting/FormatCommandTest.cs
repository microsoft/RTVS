using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Formatting;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.R.Formatting]
    public class FormatCommandTest {
        [Test]
        public void FormatDocument() {
            string content = "if(x<1){x<-2}";
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ITextView textView = new TextViewMock(textBuffer);

            var command = new FormatDocumentCommand(textView, textBuffer);
            var status = command.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.FORMATDOCUMENT);
            status.Should().Be(CommandStatus.SupportedAndEnabled);

            object o = new object();
            command.Invoke(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.FORMATDOCUMENT, null, ref o);

            string actual = textBuffer.CurrentSnapshot.GetText();
            actual.Should().Be("if (x < 1) {\r\n    x <- 2\r\n}");
        }
    }
}
