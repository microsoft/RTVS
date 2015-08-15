using System;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.EditorFactory
{
    /// <summary>
    /// An active editor instance
    /// </summary>
    public interface IEditorInstance: IDisposable
    {
        /// <summary>
        /// WPF control if editor is a custom designer and is not based on a text file.
        /// </summary>
        object WpfControl { get; }

        /// <summary>
        /// Text buffer containing document data that is to be attached to a text view. 
        /// Can be null if document is not text based.
        /// </summary>
        ITextBuffer ViewBuffer { get; }

        /// <summary>
        /// Retrieves editor instance command target for a particular view
        /// </summary>
        ICommandTarget GetCommandTarget(ITextView textView);

        /// <summary>
        /// Caption for the editor tab in the host application. Null if IDE should use default.
        /// </summary>
        string Caption { get; }

        /// <summary>
        /// Workspace item
        /// </summary>
        IWorkspaceItem WorkspaceItem { get; }
    }
}
