// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Controllers.Views;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Document {
    public static class EditorExtensions {
        public static ITextView GetFirstView(this ITextBuffer textBuffer) => TextViewConnectionListener.GetFirstViewForBuffer(textBuffer);
    }
}
