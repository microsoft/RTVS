// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;

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
                    body = string.Format(CultureInfo.InvariantCulture, Resources.MailToFrownMessage,
                                         Path.GetDirectoryName(Path.GetTempPath()), // Trims trailing slash
                                         Environment.NewLine + Environment.NewLine);
                    VsAppShell.Current.ShowMessage(body, MessageButtons.OK);
                }

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.UseShellExecute = true;
                psi.FileName = string.Format(CultureInfo.InvariantCulture, "mailto://rtvsuserfeedback@microsoft.com?subject={0}&body={1}", subject, body);
                Process.Start(psi);

                if (attachmentFile != null) {
                    IntPtr pidl = IntPtr.Zero;
                    try {
                        pidl = NativeMethods.ILCreateFromPath(attachmentFile);
                        if (pidl != IntPtr.Zero) {
                            NativeMethods.SHOpenFolderAndSelectItems(pidl, 0, IntPtr.Zero, 0);
                        }
                    } finally {
                        if (pidl != IntPtr.Zero) {
                            NativeMethods.ILFree(pidl);
                        }
                    }
                }

            } else {
                MailItem mail = outlookApp.CreateItem(OlItemType.olMailItem) as MailItem;

                mail.Subject = subject;
                mail.Body = body;
                AddressEntry currentUser = outlookApp.Session.CurrentUser.AddressEntry;
                if (currentUser.Type == "EX") {
                    mail.To = "rtvsuserfeedback@microsoft.com";
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
