// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.DataInspect.DataSource;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect.DataImport {
    /// <summary>
    /// Interaction logic for ImportDataWindow.xaml
    /// </summary>
    public partial class EnterUrl : DialogWindow {
        private string _temporaryFile;

        public EnterUrl() {
            InitializeComponent();
        }

        public string DownloadFilePath { get; private set; }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            if (!string.IsNullOrEmpty(_temporaryFile)) {
                try {
                    if (File.Exists(_temporaryFile)) {
                        File.Delete(_temporaryFile);
                    }
                } catch {
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            OkButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
            DownloadProgressBar.Visibility = Visibility.Visible;
            RunAsync().DoNotWait();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            base.Close();
        }

        private async Task RunAsync() {
            try {
                _temporaryFile = Path.GetTempFileName();
                using (var client = new WebClient()) {
                    client.DownloadProgressChanged += DownloadProgressChanged;

                    await client.DownloadFileTaskAsync(UrlTextBox.Text, _temporaryFile);
                }

                DownloadFilePath = _temporaryFile;
                OnSuccess(false);
            } catch (Exception ex) {
                OnError(ex.Message);
            }
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            DownloadProgressBar.Value = e.ProgressPercentage;
        }

        private void OnSuccess(bool preview) {
            if (preview) {
            } else {
                base.Close();
            }
        }

        private void OnError(string errorText) {
            OkButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
        }
    }
}
