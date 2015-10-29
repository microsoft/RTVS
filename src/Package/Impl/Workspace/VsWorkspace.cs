using System;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Workspace;

namespace Microsoft.VisualStudio.R.Package.Workspace {
    [Export(typeof(IWorkspace))]
    internal class VsWorkspace : IWorkspace {
        #region IWorkspace Members
#pragma warning disable 67
        /// <summary>
        /// Fires when workspace has references added or removed
        /// </summary>
        public event EventHandler<EventArgs> ReferencesChanged;
#pragma warning restore 67
        #endregion
    }
}