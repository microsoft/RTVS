using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class VariableWindowPane : ToolWindowPane
    {
        public VariableWindowPane()
        {
            Caption = Resources.VariableWindowCaption;
            Content = new VariableView();
        }
    }
}
