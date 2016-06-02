// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.Common.Core.Install;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.RClient {
    [Export(typeof(IMicrosoftRClientInstaller))]
    internal sealed class MicrosoftRClientInstaller : IMicrosoftRClientInstaller {
        private string _errorMessage;

        public void LaunchRClientSetup(ICoreShell coreShell) {
            var rClientExe = Path.Combine(Path.GetTempPath(), "RClientSetup.exe");

            LongOperationNotification.ShowWaitingPopup(Resources.DownloadingRClientInstaller, new LongAction[] {
                new LongAction() { Name = Resources.DownloadingRClientInstaller, Action = DownloadInstaller, Data = rClientExe }
            });

            if (!string.IsNullOrEmpty(_errorMessage)) {
                coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToDownloadRClient, _errorMessage));
            } else {
                // Suppress 'Operation canceled by the user' if user clicks 'No' to elevation dialog.
                try {
                    coreShell.ShowMessage(Resources.PleaseRestartVisualStudioAfterRClientSetup, MessageButtons.OK);
                    Process.Start(rClientExe);
                } catch (Win32Exception) { }
            }
        }

        private void DownloadInstaller(object data, CancellationToken ct) {
            var rClientExe = (string)data;
            try {
                using (var client = new WebClient()) {
                    client.DownloadFileAsync(
                        new Uri("http://go.microsoft.com/fwlink/?LinkId=800048", UriKind.Absolute), rClientExe);
                    while (client.IsBusy && !ct.IsCancellationRequested) {
                        Thread.Sleep(200);
                    }
                }
            } catch (WebException ex) {
                _errorMessage = ex.Message;
            }
        }
    }
}
