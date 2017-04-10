// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;

namespace Microsoft.Languages.Editor.Text {
    /// <summary>
    /// Represents editor view
    /// </summary>
    public interface IEditorView {
        /// <summary>
        /// Services attached to the view
        /// </summary>
        IServiceManager Services { get; }

        /// <summary>
        /// Property collection that holds any custom data attached to the view
        /// </summary>
        PropertyDictionary Properties { get; }

        /// <summary>
        /// Top level (view buffer)
        /// </summary>
        IEditorBuffer EditorBuffer { get; }

        /// <summary>
        /// Caret inside the view
        /// </summary>
        IViewCaret Caret { get; }

        /// <summary>
        /// Retrieves caret position for the given buffer
        /// </summary>
        ISnapshotPoint GetCaretPosition(IEditorBuffer buffer = null);

        /// <summary>
        /// Current selection in the view
        /// </summary>
        IEditorSelection Selection { get; }

        T As<T>() where T : class;
    }
}
