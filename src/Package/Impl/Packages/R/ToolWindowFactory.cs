using System;
using System.ComponentModel.Composition;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.History;

namespace Microsoft.VisualStudio.R.Packages.R {
    [Export]
    internal class ToolWindowFactory : IDisposable {
        private Lazy<IRInteractiveWorkflowProvider> WorkflowProvider { get; set; }
        private Lazy<IRHistoryWindowVisualComponentFactory> HistoryComponentFactory { get; set; }

        public bool TryCreateToolWindow(ref Guid toolWindowType, int id, out int result) {
            if (toolWindowType == RGuidList.ReplInteractiveWindowProviderGuid) {
                result = CreateInteractiveWindow(id);
                return true;
            }
            if (toolWindowType == HistoryWindowPane.WindowGuid) {
                result = CreateHistoryToolWindow(id);
                return true;
            }

            result = VSConstants.E_FAIL;
            return false;
        }

        private int CreateInteractiveWindow(int id) {
            try {
                var workflow = WorkflowProvider.Value.GetOrCreate();
                WorkflowProvider.Value.CreateInteractiveWindowAsync(workflow, id);
                return VSConstants.S_OK;
            } catch (Exception) {
                return VSConstants.E_FAIL;
            }
        }

        private int CreateHistoryToolWindow(int id) {
            try {
                var workflow = WorkflowProvider.Value.GetOrCreate();
                workflow.History.CreateVisualComponent(HistoryComponentFactory.Value, id);
                return VSConstants.S_OK;
            } catch (Exception) {
                return VSConstants.E_FAIL;
            }
        }

        public void Dispose() {
        }
    }
}