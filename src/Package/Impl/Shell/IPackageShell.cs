using System;
using Microsoft.Languages.Editor.Shell;

namespace Microsoft.VisualStudio.R.Package.Shell {
    /// <summary>
    /// Application shell provides access to services such as 
    /// composition container, export provider, global VS IDE
    /// services and so on.
    /// </summary>
    public interface IPackageShell : IEditorShell {
        string BrowseForFileOpen(IntPtr owner, string filter, string initialPath = null, string title = null);

        string BrowseForFileSave(IntPtr owner, string filter, string initialPath = null, string title = null);
    }
}
