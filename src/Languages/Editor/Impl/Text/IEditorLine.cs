// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Editor.Text {
    /// <summary>
    /// Represents code line in the editor buffer
    /// </summary>
    public interface IEditorLine: ITextRange {
        int LineNumber { get; }
        string GetText();
        string LineBreak { get; }
        IEditorBufferSnapshot Snapshot { get; }
    }
}
