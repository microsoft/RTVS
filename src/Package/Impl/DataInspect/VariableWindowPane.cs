using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    [Guid("99d2ea62-72f2-33be-afc8-b8ce6e43b5d0")]
    public class VariableWindowPane : ToolWindowPane {
        public VariableWindowPane() {
            Caption = Resources.VariableWindowCaption;
            Content = new VariableView();

            BitmapImageMoniker = KnownMonikers.VariableProperty;
        }
    }
}
