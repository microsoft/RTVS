// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.Document;
using Microsoft.R.Editor.Tree;

namespace Microsoft.R.Editor.Document {
    public interface IREditorDocument: IEditorDocument
    {
        /// <summary>
        /// Editor parse tree (object model)
        /// </summary>
        IREditorTree EditorTree { get; }

        /// <summary>
        /// Document represents content in the interactive window
        /// </summary>
        bool IsRepl { get; }

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
