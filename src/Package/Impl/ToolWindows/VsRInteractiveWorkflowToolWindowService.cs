// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.ConnectionManager.Implementation.View;
using Microsoft.R.Components.ContainerManager.Implementation.View;
using Microsoft.R.Components.PackageManager.Implementation.View;
using Microsoft.R.Components.View;

namespace Microsoft.VisualStudio.R.Package.ToolWindows {
    internal sealed class VsRInteractiveWorkflowToolWindowService : IRInteractiveWorkflowToolWindowService {
        private readonly ViewContainerToolWindowFactory _factory;

        public VsRInteractiveWorkflowToolWindowService(IServiceContainer services) {
            _factory = new ViewContainerToolWindowFactory()
                .Register<ConnectionManagerToolWindow, ConnectionManagerControl>(services)
                .Register<ContainerManagerToolWindow, ContainerManagerControl>(services)
                .Register<PackageManagerToolWindow, PackageManagerControl>(services);
        }

        public IToolWindow Connections(int instanceId = 0) => _factory.GetOrCreate<ConnectionManagerToolWindow>(instanceId);
        public IToolWindow Containers(int instanceId = 0) => _factory.GetOrCreate<ContainerManagerToolWindow>(instanceId);
        public IToolWindow Packages(int instanceId = 0) => _factory.GetOrCreate<PackageManagerToolWindow>(instanceId);
        public ViewContainerToolWindow GetOrCreate(Guid toolWindowGuid, int instanceId) => _factory.GetOrCreate(toolWindowGuid, instanceId);
    }
}