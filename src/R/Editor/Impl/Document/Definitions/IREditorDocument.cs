using System;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.R.Editor.Tree.Definitions;

namespace Microsoft.R.Editor.Document.Definitions
{
    public interface IREditorDocument: IEditorDocument
    {
        /// <summary>
        /// Editor parse tree (object model)
        /// </summary>
        IEditorTree EditorTree { get; }

        /// <summary>
        /// If trie the document is closed.
        /// </summary>
        bool IsClosed { get; }

        /// <summary>
        /// Tells of document does not have associated disk file
        /// such as when document is based off projection buffer
        /// created elsewhere as in VS Interactive Window case.
        /// </summary>
        bool IsTransient { get; }

        /// <summary>
        /// Tells document that massive change to text buffer is about to commence.
        /// Document will then stop tracking text buffer changes, will suspend
        /// it's parser and the classifier and remove all elements. Document 
        /// tree is no longer valid after this call.
        /// </summary>
        void BeginMassiveChange();

        /// <summary>
        /// Tells document that massive change to text buffer is complete. Document will perform full parse, 
        /// resume tracking of text buffer changes and classification (colorization).
        /// </summary>
        /// <returns>True if changes were made to the text buffer since call to BeginMassiveChange</returns>
        bool EndMassiveChange();

        /// <summary>
        /// Tells if massive text buffer change is currently in progress
        /// If massive change is in progress then the tree updates and 
        /// colorizer are suspended.
        /// </summary>
        bool IsMassiveChangeInProgress { get; }

        /// <summary>
        /// Fires when massive change begins
        /// </summary>
        event EventHandler<EventArgs> MassiveChangeBegun;

        /// <summary>
        /// Fires when massive change is complete
        /// </summary>
        event EventHandler<EventArgs> MassiveChangeEnded;
    }
}
