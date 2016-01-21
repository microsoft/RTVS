using System.Windows.Forms;
using Microsoft.VisualStudio.R.Package.Definitions;

namespace Microsoft.VisualStudio.R.Package.Help {
    public interface IHelpWindowVisualComponent : IVisualComponent {
        /// <summary>
        /// Browser that displays help content
        /// </summary>
        WebBrowser Browser { get; }

        void Navigate(string url);
    }
}
