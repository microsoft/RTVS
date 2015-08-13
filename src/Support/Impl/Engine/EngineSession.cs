using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Support.Engine
{
    public sealed class EngineSession : IDisposable
    {
        private Task _startingTask;
        private Process _rProcess;

        public EngineSession()
        {
            EditorShell.OnIdle += OnIdle;
        }

        public async Task<EngineResponse> SendCommand(string command)
        {
            EngineResponse response = null;

            await EnsureEngineRunningAsync();

            if (_rProcess != null)
            {
                response = new EngineResponse(_rProcess);
                _rProcess.StandardInput.WriteLine(command);
            }

            return response;
        }

        private Task EnsureEngineRunningAsync()
        {
            EditorShell.OnIdle -= OnIdle;

            if (_rProcess == null || _rProcess.HasExited)
            {
                return LaunchREngineAsync();
            }

            return Task.FromResult<object>(null);
        }

        private async void OnIdle(object sender, EventArgs e)
        {
            EditorShell.OnIdle -= OnIdle;
            await LaunchREngineAsync();
        }

        private Task LaunchREngineAsync()
        {
            if (_startingTask == null)
            {
                if (_rProcess == null || _rProcess.HasExited)
                {
                    string binPath = RToolsSettings.GetBinariesFolder();

                    if (!string.IsNullOrEmpty(binPath))
                    {
                        string exePath = Path.Combine(binPath, "RTerm.exe");

                        _startingTask = Task.Run(() =>
                        {
                            StartREngine(exePath);
                        });
                    }
                }
            }

            return Task.FromResult<object>(null);
        }

        private void StartREngine(string exePath)
        {
            ProcessStartInfo info = new ProcessStartInfo();

            info.Arguments = "--vanilla --slave --ess";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.WindowStyle = ProcessWindowStyle.Minimized;
            info.UseShellExecute = false;
            info.FileName = exePath;

            _rProcess = Process.Start(info);
            _rProcess.Exited += OnProcessExited;
            _rProcess.BeginOutputReadLine();

            _startingTask = null;
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            _rProcess.Exited -= OnProcessExited;
            _rProcess = null;
        }

        public void Dispose()
        {
            if (_rProcess != null)
            {
                _rProcess.Exited -= OnProcessExited;

                _rProcess.Kill();
                _rProcess = null;
            }
        }
    }
}
