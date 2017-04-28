// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.ViewModel {
    /// <summary>
    /// Editor view model factory. Typically imported via MEF
    /// in the host application editor factory such as in
    /// IVsEditorFactory.CreateEditorInstance.
    /// </summary>
    public interface IEditorViewModelFactory {
        /// <summary>
        /// Creates an instance of an editor over the text buffer.
        /// </summary>
        IEditorViewModel CreateEditorViewModel(ITextBuffer textBuffer);
    }
}
