// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.R.Platform.OS;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Common.Core.OS.Linux {
    public sealed class LinuxProcessServices : ProcessServices {
        /// <remarks>
        /// On Linux this calls the native strerror_r to get the message
        /// see: https://github.com/dotnet/corefx/blob/master/src/Microsoft.Win32.Primitives/src/System/ComponentModel/Win32Exception.Unix.cs
        /// </remarks>
        protected override string GetMessageFromExitCode(int processExitCode) 
            => new Win32Exception(processExitCode).Message;

        protected override void KillProcess(int pid) {
            var psi = new ProcessStartInfo {
                FileName = PathConstants.RunAsUserBinPath,
                Arguments = "-q",
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            var proc = Process.Start(psi);

            var kpm = new KillProcessMessage { ProcessId = pid };
            var json = JsonConvert.SerializeObject(kpm, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            using (var writer = new BinaryWriter(proc.StandardInput.BaseStream, Encoding.UTF8, true)) {
                var jsonBytes = Encoding.UTF8.GetBytes(json);
                writer.Write(jsonBytes.Length);
                writer.Write(jsonBytes);
                writer.Flush();
                proc.WaitForExit(1000);
            }
        }
    }
}
