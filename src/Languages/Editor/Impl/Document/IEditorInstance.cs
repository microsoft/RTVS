// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.Document {
    /// <summary>
    /// An active editor instance
    /// </summary>
    public interface IEditorInstance : IDisposable {
        /// <summary>
        /// Text buffer containing document data that is to be attached to the text view. 
        /// In languages that support projected language scenarios this is the top level
        /// projection buffer. In regular scenarios the same as the disk buffer.
        /// </summary>
        IEditorBuffer ViewBuffer { get; }

        /// <summary>
        /// Buffer that contains original content as it was retrieved from disk
        /// or generated in memory. 
        /// </summary>
        IEditorBuffer DiskBuffer { get; }

        /// <summary>
        /// Retrieves editor instance command target for a particular view
        /// </summary>
        ICommandTarget GetCommandTarget(IEditorView textView);
    }
}
