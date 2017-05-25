// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.Help;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.View;
using Microsoft.VisualStudio.R.Package.Help;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.ToolWindows;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Packages.R {
    internal sealed class RPackageToolWindowProvider {
        private readonly IServiceContainer _services;
        private readonly IRInteractiveWorkflowVisualProvider _workflowProvider;

        public RPackageToolWindowProvider(IServiceContainer services) {
            _services = services;
            _workflowProvider = _services.GetService<IRInteractiveWorkflowVisualProvider>();
        }

        public bool TryCreateToolWindow(Guid toolWindowType, int id) {
            if (toolWindowType == RGuidList.ReplInteractiveWindowProviderGuid) {
                CreateInteractiveWindow(id);
                return true;
            }

            if (toolWindowType == HistoryWindowPane.WindowGuid) {
                CreateHistoryToolWindow(id);
                return true;
            }

            if (toolWindowType == ConnectionManagerWindowPane.WindowGuid) {
                CreateConnectionManagerToolWindow(id);
                return true;
            }

            if (toolWindowType == PackageManagerWindowPane.WindowGuid) {
                CreatePackageManagerToolWindow(id);
                return true;
            }

            if (toolWindowType == PlotDeviceWindowPane.WindowGuid) {
                CreatePlotDeviceToolWindow(id);
                return true;
            }

            if (toolWindowType == PlotHistoryWindowPane.WindowGuid) {
                CreatePlotHistoryToolWindow(id);
                return true;
            }

            if (toolWindowType == HelpWindowPane.WindowGuid) {
                CreateHelpToolWindow(id);
                return true;
            }

            return false;
        }

        public ToolWindowPane CreateToolWindow(Type toolWindowType, int id) 
            => CreateContainer(toolWindowType.GUID, id) as ToolWindowPane;

        private IVisualComponentContainer<IVisualComponent> CreateContainer(Guid toolWindowType, int id) {
            if (toolWindowType == RGuidList.ReplInteractiveWindowProviderGuid) {
                return CreateInteractiveWindow(id).GetAwaiter().GetResult().Container;
            }

            if (toolWindowType == HistoryWindowPane.WindowGuid) {
                return CreateHistoryToolWindow(id).Container;
            }

            if (toolWindowType == ConnectionManagerWindowPane.WindowGuid) {
                return CreateConnectionManagerToolWindow(id).Container;
            }

            if (toolWindowType == PackageManagerWindowPane.WindowGuid) {
                return CreatePackageManagerToolWindow(id).Container;
            }

            if (toolWindowType == PlotDeviceWindowPane.WindowGuid) {
                return CreatePlotDeviceToolWindow(id).Container;
            }

            if (toolWindowType == PlotHistoryWindowPane.WindowGuid) {
                return CreatePlotHistoryToolWindow(id).Container;
            }

            if (toolWindowType == HelpWindowPane.WindowGuid) {
                return CreateHelpToolWindow(id).Container;
            }

            return null;
        }

        private Task<IInteractiveWindowVisualComponent> CreateInteractiveWindow(int id) {
            var workflow = _workflowProvider.GetOrCreate();
            return workflow.GetOrCreateVisualComponentAsync(id);
        }

        private IRHistoryWindowVisualComponent CreateHistoryToolWindow(int id) {
            var factory = _services.GetService<IRHistoryVisualComponentContainerFactory>();
            var workflow = _workflowProvider.GetOrCreate();
            return workflow.History.GetOrCreateVisualComponent(factory, id);
        }

        private IConnectionManagerVisualComponent CreateConnectionManagerToolWindow(int id) {
            var workflow = _workflowProvider.GetOrCreate();
            return workflow.Connections.GetOrCreateVisualComponent(id);
        }

        private IRPackageManagerVisualComponent CreatePackageManagerToolWindow(int id) {
            var factory = _services.GetService<IRPackageManagerVisualComponentContainerFactory>();
            var workflow = _workflowProvider.GetOrCreate();
            return workflow.Packages.GetOrCreateVisualComponent(factory, id);
        }

        private IRPlotDeviceVisualComponent CreatePlotDeviceToolWindow(int id) {
            var factory = _services.GetService<IRPlotDeviceVisualComponentContainerFactory>();
            var workflow = _workflowProvider.GetOrCreate();
            return workflow.Plots.GetOrCreateVisualComponent(factory, id);
        }

        private IRPlotHistoryVisualComponent CreatePlotHistoryToolWindow(int id) {
            var factory = _services.GetService<IRPlotHistoryVisualComponentContainerFactory>();
            var workflow = _workflowProvider.GetOrCreate();
            return workflow.Plots.GetOrCreateVisualComponent(factory, id);
        }

        private IHelpVisualComponent CreateHelpToolWindow(int id) {
            var factory = _services.GetService<IHelpVisualComponentContainerFactory>();
            return factory.GetOrCreate(id).Component;
        }
    }
}