using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Common.Core.OS {
    public sealed class DotNetProcess : IProcess {
        private readonly Process _process;

        public DotNetProcess(Process process) {
            _process = process;
        }

        public int Id => _process.Id;
        public StreamWriter StandardInput => _process.StandardInput;
        public StreamReader StandardOutput => _process.StandardOutput;
        public StreamReader StandardError => _process.StandardError;
        public bool HasExited => _process.HasExited;
        public int ExitCode => _process.ExitCode;

        public event EventHandler Exited {
            add {
                _process.EnableRaisingEvents = true;
                _process.Exited += value;
            }
            remove => _process.Exited -= value;
        }

        public void Kill() => _process.Kill();
        public bool WaitForExit(int milliseconds) => _process.WaitForExit(milliseconds);
        public void Dispose() => _process.Dispose();
    }
}