// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Editor {
    /// <summary>
    /// Represents immutable snapshot of the editor buffer
    /// </summary>
    public interface IBufferSnapshot : ITextProvider {
        T ToTextSnapshot<T>() where T : class;
        IEditorBuffer EditorBuffer { get; }
        int LineCount { get; }
        IEditorLine GetLineFromPosition(int position);
        IEditorLine GetLineFromLineNumber(int lineNumber);
        int GetLineNumberFromPosition(int position);
    }
}
