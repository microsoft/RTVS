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
            Closing += MainWindow_Closing;
        }

        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            await BrokerManager.StopBrokerInstanceAsync();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            if (Properties.Settings.Default.UseDifferentBrokerUser) {
                if (await CredentialManager.IsBrokerUserCredentialSavedAsync()) {
                    await BrokerManager.StartBrokerInstanceAsync();
                } else {
                    await CredentialManager.GetAndSaveCredentialsFromUserAsync();
                }
            } else {
                await BrokerManager.StartBrokerInstanceAsync();
            }
        }

        private async void StartBrokerBtn_Click(object sender, RoutedEventArgs e) {
            BrokerManager.ResetAutoStart();
            if (Properties.Settings.Default.UseDifferentBrokerUser) {
                if (await CredentialManager.IsBrokerUserCredentialSavedAsync()) {
                    await BrokerManager.StartBrokerInstanceAsync();
                } else {
                    await CredentialManager.GetAndSaveCredentialsFromUserAsync();
                }
            } else {
                await BrokerManager.StartBrokerInstanceAsync();
            }
        }
        private async void StopBrokerBtn_Click(object sender, RoutedEventArgs e) {
            await BrokerManager.StopBrokerInstanceAsync();
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

        private void UseBrokerUserCheckBox_Checked(object sender, RoutedEventArgs e) {
            Properties.Settings.Default.UseDifferentBrokerUser = true;
            Properties.Settings.Default.Save();
        }

        private void UseBrokerUserCheckBox_Unchecked(object sender, RoutedEventArgs e) {
            Properties.Settings.Default.UseDifferentBrokerUser = false;
            Properties.Settings.Default.Save();
        }
    }
}
