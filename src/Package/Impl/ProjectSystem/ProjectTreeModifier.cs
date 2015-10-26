using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities.Designers;
using Microsoft.VisualStudio.R.Package;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {

    [Export(typeof(IProjectTreeModifier))]
    [AppliesTo("RTools")]
    internal sealed class ProjectTreeModifier : IProjectTreeModifier {

        public IProjectTree ApplyModifications(IProjectTree tree, IProjectTreeProvider projectTreeProvider) {
            if (tree != null) {
                if (tree.Capabilities.Contains(ProjectTreeCapabilities.ProjectRoot)) {
                    tree = tree.SetIcon(ProjectIconProvider.ProjectNodeImage.ToProjectSystemType());
                } else if (tree.Capabilities.Contains(ProjectTreeCapabilities.FileOnDisk)) {
                    string ext = Path.GetExtension(tree.FilePath).ToLowerInvariant();
                    if (ext == ".r") {
                        tree = tree.SetIcon(ProjectIconProvider.RFileNodeImage.ToProjectSystemType());
                    } else if (ext == ".rdata" || ext == ".rhistory") {
                        tree = tree.SetIcon(ProjectIconProvider.RDataFileNodeImage.ToProjectSystemType());
                    }
                }
            }
            return tree;
        }
    }
}
