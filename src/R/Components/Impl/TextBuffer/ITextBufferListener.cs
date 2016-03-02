// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text {
    /// <summary>
    /// Export this interface on an object that wants to receive
    /// events when document text buffer is getting created
    /// and disposed and when view is created for the text buffer.
    /// </summary>
    public interface ITextBufferListener {
        /// <summary>
        /// Called when a text buffer gets attached to its first view
        /// </summary>
        void OnTextBufferCreated(ITextBuffer textBuffer);

        /// <summary>
        /// Called when a text buffer is detached from its last view
        /// </summary>
        void OnTextBufferDisposed(ITextBuffer textBuffer);
    }
}
