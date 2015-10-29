using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.R.Actions.Logging;
using Microsoft.R.Actions.Utility;

namespace Microsoft.R.Actions.Script {
    /// <summary>
    /// Implements execution of R command.
    /// Can be used during setup or in other scenarios
    /// where code needs to execute something in R.
    /// </summary>
    public sealed class RCommand : IDisposable {
        private Process _rProcess;
        private IActionLog _log;

        public Task Task { get; private set; }

        /// <summary>
        /// Executes command line for any R binary with arguments 
        /// </summary>
        public static RCommand ExecuteAsync(string executable, string arguments, IActionLog log) {
            RCommand command = new RCommand(log);
            command.SendCommandAsync(executable, arguments);
            return command;
        }

        /// <summary>
        /// Executes 'R CMD arguments' 
        /// </summary>
        public static RCommand ExecuteAsync(string arguments, IActionLog log) {
            return ExecuteAsync("R.exe", "CMD " + arguments, log);
        }

        /// <summary>
        /// Executes 'RScript --vanilla --slave -e' with the supplied expression
        /// </summary>
        /// <param name="msTimeout"></param>
        /// <returns>Standard output produced by RScript.exe</returns>
        public static bool ExecuteRExpression(string expression, IActionLog log, int msTimeout) {
            RCommand command = ExecuteRExpressionAsync(expression, log);
            return command.Task.Wait(msTimeout);
        }

        /// <summary>
        /// Executes 'RScript --vanilla --slave -e' with the supplied expression asynchronously
        /// </summary>
        /// <param name="msTimeout"></param>
        /// <returns>Standard output produced by RScript.exe</returns>
        public static RCommand ExecuteRExpressionAsync(string expression, IActionLog log) {
            string executable = "RScript.exe";
            string baseArgumens = "--vanilla --slave -e ";

            return ExecuteAsync(executable, baseArgumens + expression, log);
        }

        private RCommand(IActionLog log) {
            _log = log;
        }

        private void SendCommandAsync(string executable, string arguments) {
            this.Task = Task.Run(() => {
                Launch(executable, arguments);
            });
        }

        private void Launch(string executable, string arguments) {
            string binPath = RUtility.GetRBinariesFolder();

            if (!string.IsNullOrEmpty(binPath)) {
                ProcessStartInfo info = new ProcessStartInfo();

                info.FileName = Path.Combine(binPath, executable);
                info.Arguments = arguments;
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.CreateNoWindow = true;
                info.WindowStyle = ProcessWindowStyle.Hidden;

                _rProcess = new Process();
                _rProcess.StartInfo = info;

                _rProcess.Exited += OnProcessExited;
                _rProcess.OutputDataReceived += OnOutputDataReceived;
                _rProcess.ErrorDataReceived += OnErrorDataReceived;

                _rProcess.Start();

                _rProcess.BeginOutputReadLine();
                _rProcess.BeginErrorReadLine();

                _rProcess.WaitForExit();

                Dispose();
            }
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data != null) {
                _log.WriteLineAsync(MessageCategory.Error, e.Data);
            }
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data != null) {
                _log.WriteLineAsync(MessageCategory.General, e.Data);
            }
        }

        private void OnProcessExited(object sender, EventArgs e) {
            Dispose();
        }

        public void Dispose() {
            if (_rProcess != null) {
                _rProcess.Exited -= OnProcessExited;
                _rProcess.OutputDataReceived -= OnOutputDataReceived;
                _rProcess.ErrorDataReceived -= OnErrorDataReceived;

                _rProcess.CancelOutputRead();
                _rProcess.CancelErrorRead();

                if (!_rProcess.HasExited) {
                    _rProcess.CloseMainWindow();
                    _rProcess.Kill();
                    _rProcess.WaitForExit();
                }

                _rProcess.Dispose();
                _rProcess = null;
            }
        }
    }
}
