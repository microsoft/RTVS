// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Containers {
    public interface IContainer {
        string Id { get; }
        string Name { get; }
        IEnumerable<int> HostPorts { get; }
        string Status { get; }
        bool IsRunning { get; }
    }
}
