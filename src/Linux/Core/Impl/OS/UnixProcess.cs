// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Microsoft.Common.Core.OS {
    public class UnixProcess : IProcess {
        private readonly Process _process;
        public UnixProcess(Process process) {
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
            _process.Kill();
        }
    }
}
