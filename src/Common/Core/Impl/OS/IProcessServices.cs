// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace Microsoft.Common.Core.OS {
    public interface IProcessServices {
        IProcess Start(ProcessStartInfo psi);
        IProcess Start(string path);
        string MessageFromExitCode(int processExitCode);
        void Kill(IProcess process);
        void Kill(int pid);
        bool IsProcessRunning(string processName);
    }
}
