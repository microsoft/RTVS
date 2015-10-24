using System;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Interop;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal class SendMailCommand : PackageCommand {
        public SendMailCommand(Guid group, int id) :
            base(group, id) { }

        protected static void SendMail(string subject, string attachmentFile) {
            MapiMail mail = new MapiMail();
            mail.AddRecipientTo("rtvscore@microsoft.com");
            mail.AddAttachment(attachmentFile);
            mail.SendMailPopup(subject, string.Empty);
        }
    }
}
