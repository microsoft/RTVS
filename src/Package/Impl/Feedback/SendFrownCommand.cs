using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Logging;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal sealed class SendFrownCommand : SendMailCommand {
        //TODO: localize
        private const string _disclaimer =
"DISCLAIMER: Please be aware that the data contained in the attached logs contain " +
"your command history as well as all output displayed in the R Interactive Windows, " +
"and that by sending this email you are sharing this information with the RTVS team " +
"to help us diagnose the problem you are experiencing. You can open the attached .zip " +
"file in this email to inspect the contents if you have any concerns before sending the feedback.\r\n\r\n" +
"Please briefly describe what you were doing that led to the issue if applicable.";

        private const int _maxAttachmentSizeMb = 5;


        public SendFrownCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSendFrown) {
        }

        protected override void SetStatus() {
            Enabled = true;
        }

        protected override void Handle() {

            string zipPath = DiagnosticLogs.Collect();
            if (!string.IsNullOrEmpty(zipPath) && File.Exists(zipPath)) {
                if (new FileInfo(zipPath).Length > _maxAttachmentSizeMb * (1024 * 1024)) {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendFormat(Resources.AttachmentTooLarge1, _maxAttachmentSizeMb);
                    sb.Append(Environment.NewLine);
                    sb.Append(Environment.NewLine);

                    sb.Append(Resources.AttachmentTooLarge2);
                    sb.Append(Environment.NewLine);
                    sb.Append(Environment.NewLine);

                    sb.AppendFormat(Resources.AttachmentTooLarge3, DiagnosticLogs.RtvsLogZipFile, Path.GetTempPath());

                    EditorShell.Current.ShowErrorMessage(sb.ToString());
                    Process.Start(Path.GetTempPath());
                    zipPath = null;
                }

                SendMail(_disclaimer, "RTVS Frown", zipPath);
            }
        }
    }
}
