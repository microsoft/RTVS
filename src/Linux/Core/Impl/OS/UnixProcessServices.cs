using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Common.Core.OS {
    public class UnixProcessServices : IProcessServices {
        public string MessageFromExitCode(int processExitCode) {
            throw new NotImplementedException();
        }

        public Process Start(ProcessStartInfo psi) {
            return Process.Start(psi);
        }

        public Process Start(string path) {
            return Process.Start(path);
        }
    }
}
