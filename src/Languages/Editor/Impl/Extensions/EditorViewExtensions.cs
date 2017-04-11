// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor {
    public static class EditorViewExtensions {
        /// <summary>
        /// Retrieves service from the service container attached to the buffer
        /// </summary>
        public static T GetService<T>(this IEditorView editorView) where T : class
            => editorView.Services.GetService<T>();
    }
}
