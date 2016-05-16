// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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
        /// If language supports projections, the projected buffer.
        /// </summary>
        ITextBuffer ProjectedBuffer { get; }

        /// <summary>
        /// Retrieves editor instance command target for a particular view
        /// </summary>
        ICommandTarget GetCommandTarget(ITextView textView);
    }
}
