// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO.Pipes;

namespace Microsoft.R.Platform.IO {
    public interface IUserProfileNamedPipeFactory {
        NamedPipeServerStream CreatePipe(string name, int maxInstances = NamedPipeServerStream.MaxAllowedServerInstances);
    }
}
