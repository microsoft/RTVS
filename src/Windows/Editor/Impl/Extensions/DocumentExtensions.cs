// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Document {
    public static class DocumentExtensions {
        public static ITextBuffer TextBuffer(this IEditorDocument document) => document.EditorBuffer.As<ITextBuffer>();
    }
}
