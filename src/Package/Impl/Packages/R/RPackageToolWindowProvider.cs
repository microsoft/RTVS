using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.View;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Packages.R {
    [Export]
    internal class RPackageToolWindowProvider {
        [Import]
        private Lazy<IRInteractiveWorkflowProvider> WorkflowProvider { get; set; }
        [Import]
        private Lazy<IInteractiveWindowComponentContainerFactory> InteractiveWindowComponentContainerFactory { get; set; }
        [Import]
        private Lazy<IRHistoryVisualComponentContainerFactory> HistoryComponentContainerFactory { get; set; }

        public bool TryCreateToolWindow(Guid toolWindowType, int id) {
            if (toolWindowType == RGuidList.ReplInteractiveWindowProviderGuid) {
                CreateInteractiveWindow(id);
                return true;
            }

            if (toolWindowType == HistoryWindowPane.WindowGuid) {
                CreateHistoryToolWindow(id);
                return true;
            }

            return false;
        }

        public ToolWindowPane CreateToolWindow(Type toolWindowType, int id) {
            return CreateContainer(toolWindowType.GUID, id) as ToolWindowPane;
        }

        private IVisualComponentContainer<IVisualComponent> CreateContainer(Guid toolWindowType, int id) {
            if (toolWindowType == RGuidList.ReplInteractiveWindowProviderGuid) {
                return CreateInteractiveWindow(id).GetAwaiter().GetResult().Container;
            }

            if (toolWindowType == HistoryWindowPane.WindowGuid) {
                return CreateHistoryToolWindow(id).Container;
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
    }
}