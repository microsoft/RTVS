using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal class SendMailCommand : PackageCommand {
        public SendMailCommand(Guid group, int id) :
            base(group, id) { }

        protected static void SendMail(string body, string subject, string attachmentFile) {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.UseShellExecute = true;
            psi.FileName = string.Format(CultureInfo.InvariantCulture, "mailto://rtvscore@microsoft.com?subject={0}&body={1}", subject, body);
            Process.Start(psi);

            if (attachmentFile != null) {
                Process.Start(Path.GetTempPath());
            }
        }
    }
}
