using System;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Interop;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal class SendMailCommand : PackageCommand {
        public SendMailCommand(Guid group, int id) :
            base(group, id) { }

        protected static void SendMail(string body, string subject, string attachmentFile) {
            MapiMail mail = new MapiMail();
            mail.AddRecipientTo("rtvscore@microsoft.com");
            if (attachmentFile != null) {
                mail.AddAttachment(attachmentFile);
            }
            int result = mail.SendMailPopup(subject, body);
            if(result != 0) {
                string err = string.Format(
                    "Error sending e-mail: {0}.\nPlease send e-mail manually to rtvscore@microsoft.com\n"+
                    "with attached RTVSLogs.zip file that can be found in\n"+
                    "C:\\Users\\<USER_NAME>\\AppData\\Local\\Temp.", result);

                EditorShell.Current.ShowErrorMessage(err);
            }
        }
    }
}
