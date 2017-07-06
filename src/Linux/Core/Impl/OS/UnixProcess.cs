// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Common.Core.OS {
    public class UnixProcess : IProcess {
        private readonly Process _process;
        private readonly IProcessServices _ps;
        public UnixProcess(IProcessServices ps, Process process) {
            _ps = ps;
            _process = process;
        }

        public int Id => _process.Id;

        public Stream StandardInput => _process.StandardInput.BaseStream;

        public Stream StandardOutput => _process.StandardOutput.BaseStream;

        public Stream StandardError => _process.StandardError.BaseStream;

        public bool HasExited => _process.HasExited;

        public int ExitCode => _process.ExitCode;

        public event EventHandler Exited {
            add {
                _process.Exited += value;
            }
            remove {
                _process.Exited -= value;
            }
        }

        public void Kill() {
            // This is needed because broker user cannot kill process running as another user.
            KillProcess(_ps, _process.Id);
        }

        private static void KillProcess(IProcessServices ps, int pid) {
            KillProcessMessage kpm = new KillProcessMessage() { ProcessId = pid };
            string json = JsonConvert.SerializeObject(kpm, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            Process proc = UnixProcessServices.CreateRunAsUserProcess(ps, true);
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
