// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Controls;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.ConnectionManager.Implementation.View;
using Microsoft.R.Components.ContainerManager.Implementation.View;
using Microsoft.R.Components.PackageManager.Implementation.View;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.Test.Fakes.ToolWindows {
    internal sealed class TestRInteractiveWorkflowToolWindowService : IRInteractiveWorkflowToolWindowService {
        private readonly IServiceContainer _services;
        private TestToolWindow _connections;
        private TestToolWindow _containers;
        private TestToolWindow _packages;

        public TestRInteractiveWorkflowToolWindowService(IServiceContainer services) {
            _services = services;
        }

        public IToolWindow Connections(int instanceId = 0) => _connections ?? (_connections = Create<ConnectionManagerControl>());
        public IToolWindow Containers(int instanceId = 0) => _containers ?? (_containers = Create<ContainerManagerControl>());
        public IToolWindow Packages(int instanceId = 0) => _packages ?? (_packages = Create<PackageManagerControl>());

        private TestToolWindow Create<TView>()
            where TView: UserControl {
            var view = _services.CreateInstance<TView>();

            return new TestToolWindow {
                Content = view,
                ViewModel = view.DataContext
            };
        }
    }
}
