// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    /// <summary>
    /// Interaction logic for SendMailFallbackWindow.xaml
    /// </summary>
    public partial class SendMailFallbackWindow : Window {
        public SendMailFallbackWindow() {
            InitializeComponent();
        }

        public string MessageBody {
            get { return messageBodyTextBox.Text; }
            set { messageBodyTextBox.Text = value; }
        }

        private void copyToClipboardButton_Click(object sender, RoutedEventArgs e) {
            Clipboard.SetText(MessageBody);
        }
    }
}
