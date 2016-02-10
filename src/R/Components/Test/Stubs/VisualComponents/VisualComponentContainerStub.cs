using System.ComponentModel.Design;
using System.Windows;
using Microsoft.R.Components.View;

namespace Microsoft.R.Components.Test.Stubs.VisualComponents {
    public class VisualComponentContainerStub<T> : IVisualComponentContainer<T> where T : IVisualComponent {
        public T Component { get; set; }
        public bool IsOnScreen { get; set; }
        public void Show(bool focus) {
            IsOnScreen = IsOnScreen | focus;
        }

        public void UpdateCommandStatus(bool immediate) { }

        public void ShowContextMenu(CommandID commandId, Point position) { }
    }
}
