using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal class SendMailCommand : PackageCommand {
        public SendMailCommand(Guid group, int id) :
            base(group, id) { }

        protected static void SendMail(string body, string subject, string attachmentFile) {
            Application outlookApp = null;
            try {
                outlookApp = new Application();
            } catch (System.Exception) { }

            if (outlookApp == null) {
                if (attachmentFile != null) {
                    body =
@"Please attach RTVSLogs.zip file that can be found in your user TEMP folder 
and briefly describe what you were doing that led to the issue if applicable. 
Please be aware that the data contained in the attached logs contain 
your command history as well as all output displayed in the R Interactive Window";
                }

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.UseShellExecute = true;
                psi.FileName = string.Format(CultureInfo.InvariantCulture, "mailto://rtvsuserfeedback@microsoft.com?subject={0}&body={1}", subject, body);
                Process.Start(psi);

                if (attachmentFile != null) {
                    Process.Start(Path.GetTempPath());
                }
            } else {
                MailItem mail = outlookApp.CreateItem(OlItemType.olMailItem) as MailItem;

                mail.Subject = subject;
                mail.Body = body;
                AddressEntry currentUser = outlookApp.Session.CurrentUser.AddressEntry;
                if (currentUser.Type == "EX") {
                    mail.To = "rtvsuserfeedback";
                    mail.Recipients.ResolveAll();

                    if (!string.IsNullOrEmpty(attachmentFile)) {
                        mail.Attachments.Add(attachmentFile, OlAttachmentType.olByValue);
                    }

                    mail.Display(Modal: false);
                }
            }
        }
    }
}
