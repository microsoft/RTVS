// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO.Pipes;

namespace Microsoft.Common.Core.IO {
    public interface IUserProfileNamedPipeFactory {
#if NETSTANDARD1_6
        NamedPipeServerStream CreatePipe(string name, int maxInstances = -1);
#else
        NamedPipeServerStream CreatePipe(string name, int maxInstances = NamedPipeServerStream.MaxAllowedServerInstances);
#endif
    }
}
