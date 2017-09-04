// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.R.Containers;

namespace Microsoft.R.Components.Containers {
    public interface IContainerManager : IDisposable {
        IReadOnlyList<IContainer> GetContainers();
        IReadOnlyList<IContainer> GetRunningContainers();
        IDisposable SubscribeOnChanges(Action containersChanged);
    }
}