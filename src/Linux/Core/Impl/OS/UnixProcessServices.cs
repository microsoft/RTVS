// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

        // usage:
        // Microsoft.R.Host.RunAsUser [-q]
        //    -q: Quiet
        private const string RunAsUserBinPath = "/usr/lib/rtvs/Microsoft.R.Host.RunAsUser";

        public static Process CreateRunAsUserProcess(IProcessServices ps, bool quietMode) {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = RunAsUserBinPath;
            psi.Arguments = quietMode ? "-q" : "";
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;

            return ps.Start(psi);
        }
    }
}
