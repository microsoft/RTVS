using Microsoft.R.Components.View;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public class VisualComponentToolWindowAdapter<T> : IVisualComponentContainer<T> where T : IVisualComponent {
        private readonly ToolWindowPane _toolWindowPane;

        public VisualComponentToolWindowAdapter(ToolWindowPane toolWindowPane) {
            _toolWindowPane = toolWindowPane;
        }

        public T Component { get; set; }

        public bool IsOnScreen {
            get {
                if (VsWindowFrame == null) {
                    return false;
                }

                int onScreen;
                return VsWindowFrame.IsOnScreen(out onScreen) == VSConstants.S_OK && onScreen != 0;
            }
        }

        private IVsWindowFrame VsWindowFrame => _toolWindowPane.Frame as IVsWindowFrame;

        public void UpdateCommandStatus(bool immediate) {
            var shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof (SVsUIShell));
            shell.UpdateCommandUI(immediate ? 1 : 0);
        }

        public void Show(bool focus) {
            if (VsWindowFrame == null) {
                return;
            }

            if (focus) {
                ErrorHandler.ThrowOnFailure(VsWindowFrame.Show());
                Component.Control?.Focus();
            } else {
                ErrorHandler.ThrowOnFailure(VsWindowFrame.ShowNoActivate());
            }
        }
    }
}