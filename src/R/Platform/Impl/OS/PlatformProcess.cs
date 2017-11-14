// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Common.Core.OS;

namespace Microsoft.R.Platform.OS {
    public sealed class PlatformProcess : IProcess {
        private readonly Process _process;
        private readonly IProcessServices _ps;
        public PlatformProcess(IProcessServices ps, Process process) {
            _ps = ps;
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

        public void Kill() {
            // This is needed because broker user cannot kill process running as another user.
            _ps.Kill(_process.Id);
        }

        public bool WaitForExit(int milliseconds) => _process.WaitForExit(milliseconds);
        public void Dispose() => _process.Dispose();
    }
}
