using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Protocol {
    public class NamedPipeServerStreamFactory {
        public static string CreatorName = "Microsoft.R.Host.UserProfile.Creator{b101cc2d-156e-472e-8d98-b9d999a93c7a}";
        public static string DeletorName = "Microsoft.R.Host.UserProfile.Deletor{9c2aa072-7549-4992-8c17-0ccb9b8f196e}";
        public static NamedPipeServerStream Create(string name, int maxInstances = NamedPipeServerStream.MaxAllowedServerInstances) {
            PipeSecurity ps = new PipeSecurity();
            SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
            PipeAccessRule par = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
            ps.AddAccessRule(par);
            return new NamedPipeServerStream(name, PipeDirection.InOut, maxInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 1024, 1024, ps);
        }
    }
}
