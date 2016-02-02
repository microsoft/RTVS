using System.Windows;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.View;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl {
    internal class RInteractiveWindowVisualComponent : IInteractiveWindowVisualComponent {
        public ICommandTarget Controller { get; }
        public FrameworkElement Control { get; }
        public IInteractiveWindow InteractiveWindow { get; }

        public bool IsRunning => InteractiveWindow.IsRunning;
        public IWpfTextView TextView => InteractiveWindow.TextView;
        public ITextBuffer CurrentLanguageBuffer => InteractiveWindow.CurrentLanguageBuffer;

        public IVisualComponentContainer<IVisualComponent> Container { get; }

        public RInteractiveWindowVisualComponent(IInteractiveWindow interactiveWindow, IVisualComponentContainer<IInteractiveWindowVisualComponent> container) {
            InteractiveWindow = interactiveWindow;
            Container = container;

            var textView = interactiveWindow.TextView;
            Controller = ReplCommandController.FromTextView(textView);
            Control = textView.VisualElement;
            interactiveWindow.Properties.AddProperty(typeof(IInteractiveWindowVisualComponent), this);
        }

        public void Dispose() {
            InteractiveWindow.Properties.RemoveProperty(typeof (IInteractiveWindowVisualComponent));
        }
    }
}