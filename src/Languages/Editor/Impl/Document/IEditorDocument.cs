// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.Document {
    /// <summary>
    /// Common interface implemented by editor documents
    /// </summary>
    public interface IEditorDocument : IDisposable {
        /// <summary>
        /// Closes the document
        /// </summary>
        void Close();

        /// <summary>
        /// Fires when document is closing.
        /// </summary>
        event EventHandler<EventArgs> Closing;

        bool IsClosed { get; }

        /// <summary>
        /// Document text buffer
        /// </summary>
        IEditorBuffer EditorBuffer { get; }

        /// <summary>
        /// Full path to the document file. May be null or empty in transient documents.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Retrieves primary view for the document. It may or may not exist.
        /// In some environments document may have multiple views.
        /// </summary>
        IEditorView PrimaryView { get; }
    }
}
