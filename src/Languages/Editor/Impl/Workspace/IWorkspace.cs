using System;

namespace Microsoft.Languages.Editor.Workspace {
    /// <summary>
    /// Abstraction of a solution (collection of projects or Web sites). Imported to editor code via MEF. 
    /// </summary>
    public interface IWorkspace {
        /// <summary>
        /// Fires when workspace has references added or removed
        /// </summary>
        event EventHandler<EventArgs> ReferencesChanged;
    }
}
