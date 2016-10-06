// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Core;

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
            UseBrokerUserCheckBox.IsChecked = Properties.Settings.Default.UseDifferentBrokerUser;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            StartUpAsync().DoNotWait();
        }

        public static async Task StartUpAsync() {
            try {
                if (Properties.Settings.Default.UseDifferentBrokerUser) {
                    if (await CredentialManager.IsBrokerUserCredentialSavedAsync()) {
                        await BrokerManager.CreateOrAttachToBrokerInstanceAsync();
                    } else {
                        await CredentialManager.GetAndSaveCredentialsFromUserAsync();
                    }
                } else {
                    await BrokerManager.CreateOrAttachToBrokerInstanceAsync();
                }
            } catch (Exception ex) {
                SetStatusText(ex.Message);
            }
        }
        private void StartBrokerBtn_Click(object sender, RoutedEventArgs e) {
            BrokerManager.ResetAutoStart();
            StartUpAsync().DoNotWait();
        }
        private void StopBrokerBtn_Click(object sender, RoutedEventArgs e) {
            BrokerManager.StopBrokerInstanceAsync().DoNotWait();
        }

        private async void AddOrChangeBrokerUserBtn_Click(object sender, RoutedEventArgs e) {
            try {
                if (await CredentialManager.IsBrokerUserCredentialSavedAsync()) {
                    CredentialManager.RemoveCredentials();
                }
                await CredentialManager.GetAndSaveCredentialsFromUserAsync();
            } catch (Exception ex) {
                SetStatusText(ex.Message);
            }
        }
        private void RemoveBrokerUserBtn_Click(object sender, RoutedEventArgs e) {
            CredentialManager.RemoveCredentials();
        }

        public static void SetStatusText(string message) {
            _currentWindow.Dispatcher.Invoke(() => {
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
