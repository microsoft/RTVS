using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Commands
{
    internal sealed class ShowVariableWindowCommand : MenuCommand
    {
        public ShowVariableWindowCommand() :
            base((sender, args) => new Handler().OnCommand(),
                 new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdShowVariableExplorerWindow))
        {
        }

        class Handler
        {
            public void OnCommand()
            {
                ShowWindowPane(typeof(VariableWindowPane), true);
            }

            private void ShowWindowPane(Type windowType, bool focus)
            {
                var window = RPackage.Current.FindWindowPane(windowType, 0, true) as ToolWindowPane;
                if (window != null)
                {
                    var frame = window.Frame as IVsWindowFrame;
                    if (frame != null)
                    {
                        ErrorHandler.ThrowOnFailure(frame.Show());
                    }
                    if (focus)
                    {
                        var content = window.Content as System.Windows.UIElement;
                        if (content != null)
                        {
                            content.Focus();
                        }
                    }
                }
            }
        }
    }
}
