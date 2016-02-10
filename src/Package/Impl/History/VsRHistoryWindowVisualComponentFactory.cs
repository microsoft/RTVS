using System.ComponentModel.Composition;
using Microsoft.R.Components.History;
using Microsoft.R.Components.History.Implementation;
using Microsoft.R.Editor.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History {
    [Export(typeof(IRHistoryWindowVisualComponentFactory))]
    internal class VsRHistoryWindowVisualComponentFactory : IRHistoryWindowVisualComponentFactory {
        private readonly ITextEditorFactoryService _textEditorFactory;

        [ImportingConstructor]
        public VsRHistoryWindowVisualComponentFactory(ITextEditorFactoryService textEditorFactory) {
            _textEditorFactory = textEditorFactory;
        }

        public IRHistoryWindowVisualComponent Create(ITextBuffer historyTextBuffer, int instanceId) {
            var toolWindow = new HistoryWindowPane();
            var component = new RHistoryWindowVisualComponent(historyTextBuffer, _textEditorFactory, toolWindow);
            component.SetController(RMainController.FromTextView(component.TextView));
            toolWindow.Component = component;

            IVsUIShell vsUiShell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            ToolWindowUtilities.CreateToolWindow(vsUiShell, toolWindow, instanceId);

            return component;
        }
    }
}
