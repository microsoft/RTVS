// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.R.Platform.OS {
    public sealed class WindowsProcessServices : ProcessServices {
        protected override string GetMessageFromExitCode(int processExitCode)
            => ErrorCodeConverter.MessageFromErrorCode(processExitCode);

        protected override void KillProcess(int pid) {
            try {
                (Process.GetProcessById(pid)).Kill();
            } catch (ArgumentException) { }
        }
    }
}
