using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Languages.Editor.Workspace;

namespace Microsoft.VisualStudio.R.Package.Workspace {
    public interface IVsWorkspaceItem : IWorkspaceItem {
        /// <summary>
        /// Returns Visual Studio hierarchy this item belongs to
        /// </summary>
        IVsHierarchy Hierarchy { get; }

        /// <summary>
        /// Visual Studio item id in the hierarchy
        /// </summary>
        VSConstants.VSITEMID ItemId { get; }
    }
}
