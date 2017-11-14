// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Common.Core.OS;

namespace Microsoft.R.Platform.OS {
    public abstract class ProcessServices: IProcessServices {
        #region IProcessServices
        public string MessageFromExitCode(int processExitCode) 
            => GetMessageFromExitCode(processExitCode);

        public IProcess Start(ProcessStartInfo psi) {
            var process = Process.Start(psi);
            return process != null ? new PlatformProcess(this, process) : null;
        }

        public IProcess Start(string path) {
            var process = Process.Start(path);
            return process != null ? new PlatformProcess(this, process) : null;
        }

        public void Kill(IProcess process) => KillProcess(process.Id);
        public void Kill(int pid) => KillProcess(pid);
        public bool IsProcessRunning(string processName) => Process.GetProcessesByName(processName).Any();
        #endregion

        protected abstract void KillProcess(int pid); 
        protected abstract string GetMessageFromExitCode(int processExitCode);
    }
}
