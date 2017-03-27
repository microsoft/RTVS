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
        public Stream StandardInput => _process.StandardInput.BaseStream;
        public Stream StandardOutput => _process.StandardOutput.BaseStream;
        public Stream StandardError => _process.StandardError.BaseStream;
        public bool HasExited => _process.HasExited;
        public int ExitCode => _process.ExitCode;

        public event EventHandler Exited {
            add => _process.Exited += value;
            remove => _process.Exited -= value;
        }

        public void Kill() => _process.Kill();
    }
}