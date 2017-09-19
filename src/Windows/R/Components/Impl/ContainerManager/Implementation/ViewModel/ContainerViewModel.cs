// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Common.Core;
using Microsoft.R.Containers;

namespace Microsoft.R.Components.ContainerManager.Implementation.ViewModel {
    internal sealed class ContainerViewModel {
        public string Id { get; }
        public string Name { get; }
        public bool IsRunning { get; }
        public IList<int> HostPorts { get; }

        public ContainerViewModel(IContainer container) {
            Id = container.Id;
            Name = container.Name;
            IsRunning = container.IsRunning;
            HostPorts = container.HostPorts.AsList();
        }
    }
}