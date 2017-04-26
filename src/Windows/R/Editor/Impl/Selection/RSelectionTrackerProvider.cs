// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Selection;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Selection {
    [Export(typeof(ISelectionTrackerProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal sealed class RSelectionTrackerProvider: ISelectionTrackerProvider {
        public ISelectionTracker CreateSelectionTracker(IEditorView editorView, IEditorBuffer editorBuffer, ITextRange selectedRange)
            => new RSelectionTracker(editorView.As<ITextView>(), editorBuffer.As<ITextBuffer>(), selectedRange);
    }
}
