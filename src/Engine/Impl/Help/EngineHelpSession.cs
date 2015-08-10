using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Engine.Settings;

namespace Microsoft.R.Engine.Help
{
    public sealed class EngineHelpSession : IDisposable
    {
        private Task _startingTask;
        private Process _rProcess;

        public EngineHelpSession()
        {
            EditorShell.OnIdle += OnIdle;
        }

        public async Task<string> SendCommand(string command)
        {
            string result = string.Empty;

            await EnsureEngineRunningAsync();

            if (_rProcess != null)
            {
                _rProcess.StandardOutput.DiscardBufferedData();

                _rProcess.StandardInput.WriteLine(command);
                result = ReadOutput();
            }

            return result;
        }

        private string ReadOutput()
        {
            var sb = new StringBuilder();

            while(_rProcess.StandardOutput.Peek() != -1)
            {
                char ch = (char)_rProcess.StandardOutput.Read();
                sb.Append(ch);
            }

            return sb.ToString();
        }

        private Task EnsureEngineRunningAsync()
        {
            EditorShell.OnIdle -= OnIdle;

            if (_rProcess != null)
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
            if(_startingTask == null)
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
            _rProcess.Exited += OnProcess_Exited;
            _startingTask = null;
        }

        private void OnProcess_Exited(object sender, EventArgs e)
        {
            _rProcess.Exited -= OnProcess_Exited;
            _rProcess = null;
        }

        public void Dispose()
        {
            if (_rProcess != null)
            {
                _rProcess.Exited -= OnProcess_Exited;
                _rProcess.Kill();
                _rProcess = null;
            }
        }
    }
}
