// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Languages.Editor.Controllers.Views;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.ViewModel {
    [Export(typeof(IEditorViewLocator))]
    internal sealed class EditorViewLocator: IEditorViewLocator {
        public IEditorView GetPrimaryView(IEditorBuffer editorBuffer) 
            => GetAllViews(editorBuffer).FirstOrDefault();

        public IEnumerable<IEditorView> GetAllViews(IEditorBuffer editorBuffer) 
            => TextViewConnectionListener.GetViewsForBuffer(editorBuffer.As<ITextBuffer>()).Select(v => v.ToEditorView()).Where(v => v != null);
    }
}
