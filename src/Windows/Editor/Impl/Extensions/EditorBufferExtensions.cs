// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text {
    public static class EditorBufferExtensions {
        [DebuggerStepThrough]
        public static ITextSnapshot TextSnapshot(this IEditorBuffer buffer) => buffer.CurrentSnapshot.As<ITextSnapshot>();
    }
}
