using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Interpreters;
using Microsoft.UnitTests.Core;

namespace Microsoft.R.Host.Client.Test.Fixtures {
    internal class RemoteBrokerProcess {
        private const string RHostBrokerExe = "Microsoft.R.Host.Broker.Windows.exe";
        private const string RHostExe = "Microsoft.R.Host.exe";

        //private readonly IServiceContainer _services;
        private readonly string _rhostDirectory;

        private readonly IFileSystem _fileSystem;
        private readonly IRInstallationService _installations;
        private readonly IProcessServices _processServices;
        private readonly string _name;
        private readonly string _logFolder;
        private Process _brokerProcess;

        public string Address { get; private set; }
        public string Password { get; }
        
        public RemoteBrokerProcess(string name, string logFolder, IFileSystem fileSystem, IRInstallationService installations, IProcessServices processServices) {
            _name = name;
            _logFolder = logFolder;
            _fileSystem = fileSystem;
            _installations = installations;
            _processServices = processServices;
            _rhostDirectory = Path.GetDirectoryName(typeof(RHost).Assembly.GetAssemblyPath());
            Password = Guid.NewGuid().ToString();
        }

        public async Task StartAsync(Action exited) {
            var rhostExe = Path.Combine(_rhostDirectory, RHostExe);
            if (!_fileSystem.FileExists(rhostExe)) {
                throw new RHostBinaryMissingException();
            }

            var rhostBrokerExe = Path.Combine(_rhostDirectory, RHostBrokerExe);
            if (!_fileSystem.FileExists(rhostBrokerExe)) {
                throw new RHostBrokerBinaryMissingException();
            }

            var rHome = _installations.GetCompatibleEngines().First().InstallPath;

            Process process = null;
            try {
                var psi = new ProcessStartInfo {
                    FileName = rhostBrokerExe,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    Arguments =
                        $" --logging:logFolder \"{_logFolder.TrimTrailingSlash()}\"" +
                        $" --logging:logHostOutput true" +
                        $" --logging:logPackets true" +
                        $" --urls http://127.0.0.1:0" + // :0 means first available ephemeral port
                        $" --startup:name \"{_name}\"" +
                        $" --lifetime:parentProcessId {Process.GetCurrentProcess().Id}" +
                        $" --security:secret \"{Password}\"" +
                        $" --R:autoDetect false" +
                        $" --R:interpreters:test:name \"{_name}\"" +
                        $" --R:interpreters:test:basePath \"{rHome.TrimTrailingSlash()}\""
                };

                process = StartBroker(psi);
                process.EnableRaisingEvents = true;
                process.Exited += (sender, e) => exited();

                var port = ProcessUtils.GetPortByProcessId(process.Id).FirstOrDefault();
                while (port == 0) {
                    await Task.Delay(100);
                    port = ProcessUtils.GetPortByProcessId(process.Id).FirstOrDefault();
                }

                Address = $"http://127.0.0.1:{port}";
                _brokerProcess = process;
            } finally {
                if (_brokerProcess == null) {
                    try {
                        process?.Kill();
                    } catch (Exception) {



                    } finally {
                        process?.Dispose();
                    }
                }
            }
        }
        
        private Process StartBroker(ProcessStartInfo psi) {
            var process = _processServices.Start(psi);
            process.WaitForExit(250);
            if (process.HasExited && process.ExitCode < 0) {
                var message = _processServices.MessageFromExitCode(process.ExitCode);
                if (!string.IsNullOrEmpty(message)) {
                    throw new RHostDisconnectedException(Resources.Error_UnableToStartBrokerException.FormatInvariant(_name, message), new Win32Exception(message));
                }
                throw new RHostDisconnectedException(Resources.Error_UnableToStartBrokerException.FormatInvariant(_name, process.ExitCode), new Win32Exception(process.ExitCode));
            }
            return process;
        }
    }
}