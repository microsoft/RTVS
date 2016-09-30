// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;


namespace Microsoft.R.Host.Monitor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public static MainWindow _currentWindow;
        public MainWindow() {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            _currentWindow = this;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            if (await CredentialManager.IsBrokerUserCredentialSavedAsync()) {
                // start Broker process.
            } else {
                await CredentialManager.GetAndSaveCredentialsFromUserAsync();
            }
        }

        private async void StartBrokerBtn_Click(object sender, RoutedEventArgs e) {
            await BrokerManager.StartBrokerInstanceAsync();
        }
        private async void StopBrokerBtn_Click(object sender, RoutedEventArgs e) {
            await BrokerManager.StartBrokerInstanceAsync();
        }
        private async void AddOrChangeBrokerUserBtn_Click(object sender, RoutedEventArgs e) {
            if(await CredentialManager.IsBrokerUserCredentialSavedAsync()) {
                await CredentialManager.RemoveCredentialsAsync();
            }
            await CredentialManager.GetAndSaveCredentialsFromUserAsync();
        }
        private async void RemoveBrokerUserBtn_Click(object sender, RoutedEventArgs e) {
            await CredentialManager.RemoveCredentialsAsync();
        }

        public static async void SetStatusText(string message) {
            await _currentWindow.Dispatcher.InvokeAsync(() => {
                _currentWindow.StatusDetailsTextBox.Text = message;
            });
        }
    }
}
