// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.Completions {
    /// <summary>
    /// Completion context. Provides information about current document, 
    /// caret position and other necessary data for the completion engine.
    /// </summary>
    public interface IIntellisenseContext {
        IEditorIntellisenseSession Session { get; }
        IEditorBuffer EditorBuffer { get; }
        int Position { get; set; }
    }
}
