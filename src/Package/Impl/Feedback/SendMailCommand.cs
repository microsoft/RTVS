// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal class SendMailCommand : PackageCommand {
        protected IServiceContainer Services { get; }

        public SendMailCommand(Guid group, int id, IServiceContainer services) :
            base(group, id) {
            Services = services;
        }

        protected override void SetStatus() {
            Enabled = Visible = Services.GetService<ILoggingPermissions>().IsFeedbackPermitted;
        }

        protected void SendMail(string body, string subject, string attachmentFile) {
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

            Application outlookApp = null;
            try {
                outlookApp = new Application();
            } catch (System.Exception ex) {
                Services.Log().Write(LogVerbosity.Normal, MessageCategory.Error, "Unable to start Outlook: " + ex.Message);
            }

            if (outlookApp == null) {
                var fallbackWindow = new SendMailFallbackWindow {
                    MessageBody = body
                };
                fallbackWindow.Show();
                fallbackWindow.Activate();

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.UseShellExecute = true;
                psi.FileName = string.Format(
                    CultureInfo.InvariantCulture,
                    "mailto:rtvsuserfeedback@microsoft.com?subject={0}&body={1}",
                    Uri.EscapeDataString(subject),
                    Uri.EscapeDataString(body));
                Services.Process().Start(psi);
            } else {
                try {
                    MailItem mail = outlookApp.CreateItem(OlItemType.olMailItem) as MailItem;
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.To = "rtvsuserfeedback@microsoft.com";
                    mail.Display(Modal: false);
                } catch (System.Exception ex) {
                    Services.Log().Write(LogVerbosity.Normal, MessageCategory.Error, "Error composing Outlook e-mail: " + ex.Message);
                }
            }
        }
    }
}
