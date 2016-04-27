// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.R.Components.Help;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.View;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Help;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.PackageManager;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Packages.R {
    [Export]
    [Export(typeof(IPackageLibraryViewer))]
    internal class RPackageToolWindowProvider : IPackageLibraryViewer {
        [Import]
        private Lazy<IRInteractiveWorkflowProvider> WorkflowProvider { get; set; }
        [Import]
        private Lazy<IInteractiveWindowComponentContainerFactory> InteractiveWindowComponentContainerFactory { get; set; }
        [Import]
        private Lazy<IRHistoryVisualComponentContainerFactory> HistoryComponentContainerFactory { get; set; }
        [Import]
        private Lazy<IRPackageManagerVisualComponentContainerFactory> PackageManagerComponentContainerFactory { get; set; }
        [Import]
        private Lazy<IHelpVisualComponentContainerFactory> HelpVisualComponentContainerFactory { get; set; }

        public bool TryCreateToolWindow(Guid toolWindowType, int id) {
            if (toolWindowType == RGuidList.ReplInteractiveWindowProviderGuid) {
                CreateInteractiveWindow(id);
                return true;
            }

            if (toolWindowType == HistoryWindowPane.WindowGuid) {
                CreateHistoryToolWindow(id);
                return true;
            }

            if (toolWindowType == PackageManagerWindowPane.WindowGuid) {
                CreatePackageManagerToolWindow(id);
                return true;
            }

            if (toolWindowType == HelpWindowPane.WindowGuid) {
                CreateHelpToolWindow(id);
                return true;
            }

            return false;
        }

        public ToolWindowPane CreateToolWindow(Type toolWindowType, int id) {
            return CreateContainer(toolWindowType.GUID, id) as ToolWindowPane;
        }

        #region IPackageLibraryViewer
        public void ViewLibrary() {
            CreatePackageManagerToolWindow(0).Container.Show(true);
        }
        #endregion

        private IVisualComponentContainer<IVisualComponent> CreateContainer(Guid toolWindowType, int id) {
            if (toolWindowType == RGuidList.ReplInteractiveWindowProviderGuid) {
                return CreateInteractiveWindow(id).GetAwaiter().GetResult().Container;
            }

            if (toolWindowType == HistoryWindowPane.WindowGuid) {
                return CreateHistoryToolWindow(id).Container;
            }

            if (toolWindowType == PackageManagerWindowPane.WindowGuid) {
                return CreatePackageManagerToolWindow(id).Container;
            }

            if (toolWindowType == HelpWindowPane.WindowGuid) {
                return CreateHelpToolWindow(id).Container;
            }

            return null;
        }

        private Task<IInteractiveWindowVisualComponent> CreateInteractiveWindow(int id) {
            var workflow = WorkflowProvider.Value.GetOrCreate();
            return workflow.GetOrCreateVisualComponent(InteractiveWindowComponentContainerFactory.Value, id);
        }

        private IRHistoryWindowVisualComponent CreateHistoryToolWindow(int id) {
            var workflow = WorkflowProvider.Value.GetOrCreate();
            return workflow.History.GetOrCreateVisualComponent(HistoryComponentContainerFactory.Value, id);
        }

        private IRPackageManagerVisualComponent CreatePackageManagerToolWindow(int id) {
            var workflow = WorkflowProvider.Value.GetOrCreate();
            return workflow.Packages.GetOrCreateVisualComponent(PackageManagerComponentContainerFactory.Value, id);
        }

        private IHelpVisualComponent CreateHelpToolWindow(int id) {
            return HelpVisualComponentContainerFactory.Value.GetOrCreate(id).Component;
        }
    }
}