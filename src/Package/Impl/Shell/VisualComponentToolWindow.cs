using System.ComponentModel.Design;
using System.Windows;
using Microsoft.R.Components.View;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public abstract class VisualComponentToolWindow<T> : ToolWindowPane, IVisualComponentContainer<T> where T : IVisualComponent {
        private readonly VisualComponentToolWindowAdapter<T> _adapter;

        public T Component {
            get { return _adapter.Component; }
            internal set {
                _adapter.Component = value;
                Content = value.Control;
            }
        }

        public bool IsOnScreen => _adapter?.IsOnScreen ?? false;

        protected VisualComponentToolWindow() {
            _adapter = new VisualComponentToolWindowAdapter<T>(this);
        }

        public void Show(bool focus) {
            _adapter?.Show(focus);
        }

        public void ShowContextMenu(CommandID commandId, Point position) {
            _adapter?.ShowContextMenu(commandId, position);
        }

        public void UpdateCommandStatus(bool immediate) {
            _adapter?.UpdateCommandStatus(immediate);
        }
    }
}