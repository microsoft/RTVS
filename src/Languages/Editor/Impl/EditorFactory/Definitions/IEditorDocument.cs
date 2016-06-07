// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.EditorFactory {
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
        event EventHandler<EventArgs> DocumentClosing;

        /// <summary>
        /// Document text buffer
        /// </summary>
        ITextBuffer TextBuffer { get; }

        /// <summary>
        /// Full path to the document file. May be null or empty in transient documents.
        /// </summary>
        string FilePath { get; }
    }
}
