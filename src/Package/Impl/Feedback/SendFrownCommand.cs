using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal sealed class SendFrownCommand : SendMailCommand {
        public SendFrownCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSendFrown) {
        }

        protected override void SetStatus() {
            Enabled = true;
        }

        protected override void Handle() {
            string zipName = CollectAndZipLogs();
            SendMail("RTVS Frown", zipName);
        }

        private string CollectAndZipLogs() {
            try {
                List<string> logFiles = new List<string>();

                IEnumerable<string> logs = GetRecentLogFiles("Microsoft.R.Host*.log");
                logFiles.AddRange(logs);

                logs = GetRecentLogFiles("Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring*.log");
                logFiles.AddRange(logs);

                string roamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string vsActivityLog = Path.Combine(roamingFolder, @"Microsoft\VisualStudio\14.0\ActivityLog.xml");
                if (File.Exists(vsActivityLog)) {
                    logFiles.Add(vsActivityLog);
                }

                return ZipFiles(logFiles);
            } catch (System.Exception ex) {
                EditorShell.Current.ShowErrorMessage(
                    string.Format(CultureInfo.InvariantCulture, Resources.Error_CannotCollectLogs, ex.Message));
            }

            return string.Empty;
        }

        private string ZipFiles(IEnumerable<string> files) {
            string zipPath = Path.Combine(Path.GetTempPath(), "RTVSLogs.zip");

            using (FileStream fs = File.Create(zipPath)) {
                using (ZipArchive zipArchive = new ZipArchive(fs, ZipArchiveMode.Create)) {
                    foreach (string file in files) {
                        zipArchive.CreateEntryFromFile(file, Path.GetFileName(file));
                    }
                }
            }

            return zipPath;
        }

        private IEnumerable<string> GetRecentLogFiles(string pattern) {
            string tempPath = Path.GetTempPath();

            var logs = Directory.EnumerateFiles(tempPath, pattern);
            return logs.Select((file) => {
                DateTime writeTime = File.GetLastWriteTimeUtc(file);
                TimeSpan difference = DateTime.Now.ToUniversalTime() - writeTime;
                if (difference.TotalDays < 3) {
                    return file;
                }

                return null;
            });
        }

        private static async void SendMail(string attachmentFile) {
            Application outlookApp = new Application();
            if (outlookApp == null) {
                EditorShell.Current.ShowErrorMessage(Resources.Error_CannotFindOutlook);
                return;
            }

            MailItem mail = await outlookApp.CreateItem(OlItemType.olMailItem) as MailItem;

            mail.Subject = "RTVS feedback";
            AddressEntry currentUser = outlookApp.Session.CurrentUser.AddressEntry;
            if (currentUser.Type == "EX") {
                ExchangeUser manager = currentUser.GetExchangeUser().GetExchangeUserManager();

                mail.Recipients.Add(manager.PrimarySmtpAddress);
                mail.Recipients.ResolveAll();

                mail.Attachments.Add(attachmentFile, OlAttachmentType.olByValue);
                mail.Send();
            }
        }
    }
}
