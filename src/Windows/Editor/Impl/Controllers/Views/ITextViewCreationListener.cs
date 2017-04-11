// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Controllers.Views {
    /// <summary>
    /// Exported via MEF for a particular content type by code that is interested in tracking view creation.
    /// Interface is called for every view of this content type is created and received aggregate focus.
    /// Useful for initialization of components that depend on the view, such as tag navigator, selection
    /// container, F1 help context tracker and so on.
    /// </summary>
    public interface ITextViewCreationListener {
        void OnTextViewCreated(ITextView textView, ITextBuffer textBuffer);
    }
}
