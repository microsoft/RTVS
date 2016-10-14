// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Common.Core;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Monitor.Logging;

namespace Microsoft.R.Host.Monitor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        public MainWindow() {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            UseBrokerUserCheckBox.IsChecked = Properties.Settings.Default.UseDifferentBrokerUser;

            _loggerFactory = new LoggerFactory();
            _loggerFactory
                .AddDebug()
                .AddProvider(new MonitorLoggerProvider());
            _logger = _loggerFactory.CreateLogger<MainWindow>();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            StartUpAsync().DoNotWait();
        }

        public async Task StartUpAsync() {
            try {
                int id = 0;
                if (Properties.Settings.Default.UseDifferentBrokerUser) {
                    if (await CredentialManager.IsBrokerUserCredentialSavedAsync(_logger)) {
                        id = await BrokerManager.CreateOrAttachToBrokerInstanceAsync(_logger);
                    } else {
                        await SetErrorTextAsync(Monitor.Resources.Info_DidNotFindSavedCredentails);
                        var count = await CredentialManager.GetAndSaveCredentialsFromUserAsync(_logger);
                        if (count >= CredentialManager.MaxCredUIAttempts) {
                            await SetErrorTextAsync(Monitor.Resources.Info_TooManyLoginAttempts);
                        }
                    }
                } else {
                    id = await BrokerManager.CreateOrAttachToBrokerInstanceAsync(_logger);
                }

                if(id!= 0) {
                    await SetStatusTextAsync(Monitor.Resources.Status_BrokerStarted.FormatInvariant(id));
                }
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _logger?.LogError(Monitor.Resources.Error_StartUpFailed, ex.Message);
                await SetErrorTextAsync(ex.Message);
            }
        }
        private void StartBrokerBtn_Click(object sender, RoutedEventArgs e) {
            BrokerManager.ResetAutoStart();
            StartUpAsync().DoNotWait();
        }
        private void StopBrokerBtn_Click(object sender, RoutedEventArgs e) {
            BrokerManager.StopBrokerInstanceAsync(_logger).DoNotWait();
            SetStatusTextAsync(Monitor.Resources.Status_BrokerStopped).Task.DoNotWait();
        }

        private async void AddOrChangeBrokerUserBtn_Click(object sender, RoutedEventArgs e) {
            try {
                if (await CredentialManager.IsBrokerUserCredentialSavedAsync(_logger)) {
                    CredentialManager.RemoveCredentials(_logger);
                }
                var count = await CredentialManager.GetAndSaveCredentialsFromUserAsync(_logger);
                if(count >= CredentialManager.MaxCredUIAttempts) {
                    await SetErrorTextAsync(Monitor.Resources.Info_TooManyLoginAttempts);
                }
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _logger?.LogError(Monitor.Resources.Error_AddOrChangeUserFailed, ex.Message);
                await SetErrorTextAsync(ex.Message);
            }
        }
        private void RemoveBrokerUserBtn_Click(object sender, RoutedEventArgs e) {
            CredentialManager.RemoveCredentials(_logger);
        }

        public System.Windows.Threading.DispatcherOperation SetStatusTextAsync(string message) {
            return Dispatcher.InvokeAsync(() => {
                StatusDetailsTextBox.Text = message;
                StatusDetailsTextBox.Foreground = Brushes.Green;
            });
        }

        public System.Windows.Threading.DispatcherOperation SetErrorTextAsync(string message) {
            return Dispatcher.InvokeAsync(() => {
                StatusDetailsTextBox.Text = message;
                StatusDetailsTextBox.Foreground = Brushes.Red;
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
