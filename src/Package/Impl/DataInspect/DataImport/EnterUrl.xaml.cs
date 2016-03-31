// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.R.Package.DataInspect.DataImport {
    /// <summary>
    /// Interaction logic for ImportDataWindow.xaml
    /// </summary>
    public partial class EnterUrl : DialogWindow {
        public EnterUrl() {
            InitializeComponent();
        }

        public string DownloadFilePath { get; private set; }

        public string Name { get; private set; }

        public void DeleteTemporaryFile() {
            if (!string.IsNullOrEmpty(DownloadFilePath)) {
                try {
                    if (File.Exists(DownloadFilePath)) {
                        File.Delete(DownloadFilePath);
                        DownloadFilePath = null;
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
                var temporaryFile = Path.GetTempFileName();
                Uri uri = new Uri(UrlTextBox.Text);
                using (var client = new WebClient()) {
                    client.DownloadProgressChanged += DownloadProgressChanged;

                    await client.DownloadFileTaskAsync(uri, temporaryFile);
                }

                DownloadFilePath = temporaryFile;
                Name = Path.GetFileNameWithoutExtension(uri.Segments[uri.Segments.Length - 1]);
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
