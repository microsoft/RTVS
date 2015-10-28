using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Logging;

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
            if (result != 0) {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat(Resources.Error_CannotSendFeedback1, result, Enum.GetName(typeof(MapiErrorCode), result));
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.AppendFormat(Resources.Error_CannotSendFeedback2, DiagnosticLogs.RtvsLogZipFile, Path.GetTempPath());

                EditorShell.Current.ShowErrorMessage(sb.ToString());
                Process.Start(Path.GetTempPath());
            }
        }
    }
}
