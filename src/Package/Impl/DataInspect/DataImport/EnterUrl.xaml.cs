// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.R.Package.Wpf;

namespace Microsoft.VisualStudio.R.Package.DataInspect.DataImport
{
    /// <summary>
    /// Interaction logic for ImportDataWindow.xaml
    /// </summary>
    public partial class EnterUrl : PlatformDialogWindow {
        private WebClient _client;

        public EnterUrl() {
            InitializeComponent();
        }

        public string DownloadFilePath { get; private set; }

        public string VariableName { get; private set; }

        public void DeleteTemporaryFile() {
            if (!string.IsNullOrEmpty(DownloadFilePath)) {
                try {
                    File.Delete(DownloadFilePath);
                    DownloadFilePath = null;
                } catch {
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            DoOK();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            DoCancel();
        }

        private void DoOK() {
            OkButton.IsEnabled = false;
            DownloadProgressBar.Value = 0;
            UrlTextBox.Visibility = Visibility.Hidden;
            DownloadProgressBar.Visibility = Visibility.Visible;
            ErrorBlock.Visibility = Visibility.Collapsed;
            ErrorText.Text = null;
            RunAsync().DoNotWait();
        }

        private void DoCancel() {
            var client = _client;
            if (client == null) {
                Close();
                return;
            }

            client.CancelAsync();
            UrlTextBox.Visibility = Visibility.Visible;
            DownloadProgressBar.Visibility = Visibility.Collapsed;
            ErrorBlock.Visibility = Visibility.Collapsed;
        }

        private async Task RunAsync() {
            try {
                var temporaryFile = Path.GetTempFileName();
                var uri = new Uri(UrlTextBox.Text);
                using (var client = new WebClient()) {
                    _client = client;
                    client.DownloadProgressChanged += DownloadProgressChanged;
                    await client.DownloadFileTaskAsync(uri, temporaryFile);
                    _client = null;
                }

                DownloadFilePath = temporaryFile;
                VariableName = Path.GetFileNameWithoutExtension(uri.Segments[uri.Segments.Length - 1]);
                OnSuccess();
            } catch (Exception ex) when (!(ex is OperationCanceledException)) {
                OnError(ex.Message);
            }
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            DownloadProgressBar.Value = e.ProgressPercentage;
        }

        private void OnSuccess() {
            Close();
        }

        private void OnError(string errorText) {
            OkButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
            ErrorText.Text = errorText;

            UrlTextBox.Visibility = Visibility.Visible;
            DownloadProgressBar.Visibility = Visibility.Collapsed;
            ErrorBlock.Visibility = Visibility.Visible;
        }

        private void OkButton_KeyUp(object sender, KeyEventArgs e) {
            if(e.Key == System.Windows.Input.Key.Enter) {
                DoOK();
            }
        }

        private void CancelButton_KeyUp(object sender, KeyEventArgs e) {
            if(e.Key == System.Windows.Input.Key.Escape) {
                DoCancel();
            }
        }
    }
}
