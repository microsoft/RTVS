using System;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.EditorFactory {
    /// <summary>
    /// An active editor instance
    /// </summary>
    public interface IEditorInstance : IDisposable {
        /// <summary>
        /// Text buffer containing document data that is 
        /// to be attached to a text view. 
        /// </summary>
        ITextBuffer ViewBuffer { get; }

        /// <summary>
        /// Retrieves editor instance command target for a particular view
        /// </summary>
        ICommandTarget GetCommandTarget(ITextView textView);

        /// <summary>
        /// Caption for the editor tab in the host application. 
        /// Null if IDE should use default.
        /// </summary>
        string Caption { get; }

        /// <summary>
        /// Workspace item that represents document 
        /// in the host application project system.
        /// </summary>
        IWorkspaceItem WorkspaceItem { get; }
    }
}
