// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace Microsoft.Common.Core.OS {
    public sealed class ProcessServices : IProcessServices {
        public IProcess Start(ProcessStartInfo psi) {
            var process = Process.Start(psi);
            return process != null ? new DotNetProcess(process) : null;
        }

        public IProcess Start(string path) {
            var process = Process.Start(path);
            return process != null ? new DotNetProcess(process) : null;
        }

        public string MessageFromExitCode(int processExitCode) => ErrorCodeConverter.MessageFromErrorCode(processExitCode);

        public void Kill(IProcess process) => process.Kill();
        public void Kill(int pid) => (Process.GetProcessById(pid)).Kill();
    }
}
