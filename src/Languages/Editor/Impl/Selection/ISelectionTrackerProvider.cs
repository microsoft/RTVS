// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.Selection {
    /// <summary>
    /// Factory for selection tracker that assist in preserving selection
    /// and caret positioning during range formatting and automatic formatting.
    /// Exported via MEF in Visual Studio for a given content type or provided as a service.
    /// </summary>
    public interface ISelectionTrackerProvider {
        ISelectionTracker CreateSelectionTracker(IEditorView editorView, IEditorBuffer editorBuffer, ITextRange selectedRange);
    }
}
