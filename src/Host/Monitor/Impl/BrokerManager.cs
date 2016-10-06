// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Monitor {
    public class BrokerManager {
        private const string RHostBroker = "Microsoft.R.Host.Broker";
        private static string RHostBrokerExe = $"{RHostBroker}.exe";
        private static string RHostBrokerConfig = $"{RHostBroker}.Config.json";
        private static Process _brokerProcess;
       
        public static bool AutoRestart { get; set; }
        public static int AutoRestartMaxCount {
            get {
                return Properties.Settings.Default.AutoRestartMaxCount;
            }
            set {
                Properties.Settings.Default.AutoRestartMaxCount = value;
            }
        }

        private static int _autoRestartCount = 0;

        static BrokerManager() {
            AutoRestart = true;
        }

        public static Task CreateOrAttachToBrokerInstanceAsync() {
            return Task.Run(async () => {
                await StopBrokerInstanceAsync();
                Process[] processes = Process.GetProcessesByName(RHostBroker);
                if (processes.Length > 0) {
                    _brokerProcess = processes[0];
                    _brokerProcess.EnableRaisingEvents = true;
                    _brokerProcess.Exited += ProcessExited;
                } else {
                    string assemblyRoot = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
                    string rBrokerExePath = Path.Combine(assemblyRoot, RHostBrokerExe);
                    string configFilePath = Path.Combine(assemblyRoot, RHostBrokerConfig);

                    ProcessStartInfo psi = new ProcessStartInfo(rBrokerExePath);
                    psi.Arguments = $"--config \"{configFilePath}\"";
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = false;
                    psi.WorkingDirectory = assemblyRoot;

                    if (Properties.Settings.Default.UseDifferentBrokerUser) {
                        await CredentialManager.SetCredentialsOnProcessAsync(psi);
                    }
                    
                    _brokerProcess = new Process() { StartInfo = psi };
                    _brokerProcess.EnableRaisingEvents = true;
                    _brokerProcess.Exited += ProcessExited;
                    _brokerProcess.Start();
                }

                AutoRestart = true;
                MainWindow.SetStatusText($"Broker Process running ... {_brokerProcess.Id}");
            });
        }

        public static void ResetAutoStart() {
            _autoRestartCount = 0;
        }

        private static void ProcessExited(object sender, EventArgs e) {
            if (AutoRestart && ++_autoRestartCount <= AutoRestartMaxCount) {
                CreateOrAttachToBrokerInstanceAsync().DoNotWait();
            }
        }

        public static Task StopBrokerInstanceAsync() {
            return Task.Run(() => {
                try {
                    AutoRestart = false;
                    _brokerProcess?.Kill();
                    _brokerProcess = null;
                    MainWindow.SetStatusText($"Broker Process {_brokerProcess.Id} Stopped.");
                } catch (Exception) {
                }
            });
        }

        
    }
}
