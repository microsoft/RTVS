// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO.Pipes;
using System.Security.Principal;
using Microsoft.R.Platform.IO;

namespace Microsoft.R.Host.Protocol.Test.UserProfileServicePipe {
    public class UserProfileTestNamedPipeTestStreamFactory : IUserProfileNamedPipeFactory {
        public static string CreatorName = "Microsoft.R.Host.UserProfile.Creator{b101cc2d-156e-472e-8d98-b9d999a93c7a}";
        public static string DeletorName = "Microsoft.R.Host.UserProfile.Deletor{9c2aa072-7549-4992-8c17-0ccb9b8f196e}";

        public NamedPipeServerStream CreatePipe(string name, int maxInstances = NamedPipeServerStream.MaxAllowedServerInstances) {
            PipeSecurity ps = new PipeSecurity();
            SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            PipeAccessRule par = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
            ps.AddAccessRule(par);
            return new NamedPipeServerStream(name, PipeDirection.InOut, maxInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 1024, 1024, ps);
        }
    }
}
