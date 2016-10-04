// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Monitor {
    public class BrokerManager {
        private const string RHostBroker = "Microsoft.R.Host.Broker";
        private static string RHostBrokerExe = $"{RHostBroker}.exe";
        private static string RHostBrokerconfig = $"{RHostBroker}.Config.json";
        private static Process _brokerProcess;
       
        public static bool AutoRestart { get; set; }
        public static int AutoRestartCount {
            get {
                return Properties.Settings.Default.AutoRestartCount;
            }
            set {
                Properties.Settings.Default.AutoRestartCount = value;
            }
        }

        private static int _autoRestartCount = 0;

        static BrokerManager() {
            AutoRestart = true;
        }

        public static Task StartBrokerInstanceAsync() {
            return Task.Run(async () => {
                await StopBrokerInstanceAsync();
                Process[] processes = Process.GetProcessesByName(RHostBroker);
                if (processes.Length > 0) {
                    _brokerProcess = processes[0];
                } else {
                    string assemblyRoot = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
                    string rBrokerExePath = Path.Combine(assemblyRoot, RHostBrokerExe);
                    string configFilePath = Path.Combine(assemblyRoot, RHostBrokerconfig);

                    ProcessStartInfo psi = new ProcessStartInfo(rBrokerExePath);
                    psi.Arguments = $"--config \"{configFilePath}\"";
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = false;
                    psi.WorkingDirectory = assemblyRoot;

                    if (Properties.Settings.Default.UseDifferentBrokerUser) {
                        await CredentialManager.SetCredentialsOnProcess(psi);
                    }
                    
                    _brokerProcess = new Process() { StartInfo = psi };

                    _brokerProcess.Start();
                }

                _brokerProcess.EnableRaisingEvents = true;
                MainWindow.SetStatusText($"Broker Process running ... {_brokerProcess.Id}");
                _brokerProcess.Exited += _brokerProcess_Exited;
                AutoRestart = true;
            });
        }

        public static void ResetAutoStart() {
            _autoRestartCount = 0;
        }


        private static async void _brokerProcess_Exited(object sender, EventArgs e) {
            if (AutoRestart && ++_autoRestartCount <= AutoRestartCount) {
                await StartBrokerInstanceAsync();
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
