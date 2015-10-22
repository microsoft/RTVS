using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal class SendMailCommand : PackageCommand {
        public SendMailCommand(Guid group, int id) :
            base(group, id) { }

        protected static void SendMail(string subject, string attachmentFile) {
            Application outlookApp = new Application();
            if (outlookApp == null) {
                EditorShell.Current.ShowErrorMessage(Resources.Error_CannotFindOutlook);
                Process.Start(Path.GetTempPath());
                return;
            }

            MailItem mail = outlookApp.CreateItem(OlItemType.olMailItem) as MailItem;

            mail.Subject = subject;
            AddressEntry currentUser = outlookApp.Session.CurrentUser.AddressEntry;
            if (currentUser.Type == "EX") {
                mail.To = "rtvscore";
                mail.Recipients.ResolveAll();

                if (!string.IsNullOrEmpty(attachmentFile)) {
                    mail.Attachments.Add(attachmentFile, OlAttachmentType.olByValue);
                }

                mail.Display(Modal: false);
            }
        }
    }
}
