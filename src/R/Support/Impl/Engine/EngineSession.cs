using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Actions.Utility;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Support.Engine {
    public sealed class EngineSession : IDisposable {
        private Task _startingTask;
        private Process _rProcess;
        private Func<string, object, object> _dataConverter;

        public EngineSession(Func<string, object, object> dataConverter) : this() {
            _dataConverter = dataConverter;
        }

        public EngineSession() {
            EditorShell.OnIdle += OnIdle;
        }

        public EngineResponse SendCommand(string command, object p, Action<object> dataReadyCallback) {
            EngineResponse response = null;

            EnsureEngineRunning();

            if (_rProcess != null) {
                response = new EngineResponse(_rProcess, dataReadyCallback, _dataConverter, p);

                // $$$ trailer is to force error output which will help us
                // to determine that the command output was fully received
                _rProcess.StandardInput.WriteLine(command + ";\n $$$");
            }

            return response;
        }

        private void EnsureEngineRunning() {
            EditorShell.OnIdle -= OnIdle;

            if (_rProcess == null || _rProcess.HasExited) {
                LaunchREngine();
            }
        }

        private void OnIdle(object sender, EventArgs e) {
            EditorShell.OnIdle -= OnIdle;
            Task.Run(() => LaunchREngine());
        }

        private void LaunchREngine() {
            if (_startingTask == null) {
                if (_rProcess == null || _rProcess.HasExited) {
                    string binPath = RInstallation.GetBinariesFolder(RToolsSettings.Current.RBasePath);

                    if (!string.IsNullOrEmpty(binPath)) {
                        string exePath = Path.Combine(binPath, "RTerm.exe");
                        StartREngine(exePath);
                    }
                }
            }
        }

        private void StartREngine(string exePath) {
            ProcessStartInfo info = new ProcessStartInfo();

            info.Arguments = "--vanilla --slave --ess";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = false;
            info.FileName = exePath;

            _rProcess = Process.Start(info);
            _rProcess.Exited += OnProcessExited;

            _rProcess.BeginOutputReadLine();
            _rProcess.BeginErrorReadLine();

            _startingTask = null;
        }

        private void OnProcessExited(object sender, EventArgs e) {
            _rProcess.Exited -= OnProcessExited;
            _rProcess = null;
        }

        public void Dispose() {
            if (_rProcess != null) {
                _rProcess.Exited -= OnProcessExited;

                _rProcess.CancelOutputRead();
                _rProcess.CancelErrorRead();

                _rProcess.CloseMainWindow();
                _rProcess.Kill();
                _rProcess.WaitForExit();

                _rProcess.Dispose();

                _rProcess = null;
            }
        }
    }
}
