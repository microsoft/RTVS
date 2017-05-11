// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Microsoft.Common.Core.OS {
    public class UnixProcessServices : IProcessServices {
        public string MessageFromExitCode(int processExitCode) {
            // on linux this calls the native strerror_r to get the message
            // see: https://github.com/dotnet/corefx/blob/master/src/Microsoft.Win32.Primitives/src/System/ComponentModel/Win32Exception.Unix.cs
            Win32Exception ex = new Win32Exception(processExitCode);
            return ex.Message;
        }

        public Process Start(ProcessStartInfo psi) {
            return Process.Start(psi);
        }

        public Process Start(string path) {
            return Process.Start(path);
        }
    }
}
