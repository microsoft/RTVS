// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Diagnostics;
using Microsoft.R.Platform.OS;

namespace Microsoft.Common.Core.OS.Mac {
    public sealed class MacProcessServices : ProcessServices {
        /// <remarks>
        /// On Unix this calls the native strerror_r to get the message
        /// see: https://github.com/dotnet/corefx/blob/master/src/Microsoft.Win32.Primitives/src/System/ComponentModel/Win32Exception.Unix.cs
        /// </remarks>
        protected override string GetMessageFromExitCode(int processExitCode) 
            => new Win32Exception(processExitCode).Message;

        protected override void KillProcess(int pid) 
            => Process.GetProcessById(pid)?.Kill();
    }
}
