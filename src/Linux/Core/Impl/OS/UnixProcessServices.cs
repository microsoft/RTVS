// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

        public void Kill(IProcess process) {
            Kill(process.Id);
        }

        public void Kill(int pid) {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = PathConstants.RunAsUserBinPath;
            psi.Arguments = "-q";
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;

            var proc = Process.Start(psi);

            KillProcessMessage kpm = new KillProcessMessage() { ProcessId = pid };
            string json = JsonConvert.SerializeObject(kpm, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            using (BinaryWriter writer = new BinaryWriter(proc.StandardInput.BaseStream, Encoding.UTF8, true)) {
                var jsonBytes = Encoding.UTF8.GetBytes(json);
                writer.Write(jsonBytes.Length);
                writer.Write(jsonBytes);
                writer.Flush();
                proc.WaitForExit(1000);
            }
        }
    }
}
