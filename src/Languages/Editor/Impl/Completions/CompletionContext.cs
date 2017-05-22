// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// Completion context. Provides information about current document, 
    /// caret position and other necessary data for the completion engine.
    /// </summary>
    public class IntellisenseContext : IIntellisenseContext {
        public int Position { get; set; }
        public IEditorIntellisenseSession Session { get; }
        public IEditorBuffer EditorBuffer { get; }
 
        public IntellisenseContext(IEditorIntellisenseSession session, IEditorBuffer editorBuffer, int position) {
            Session = session;
            EditorBuffer = editorBuffer;
            Position = position;
        }
    }
}
