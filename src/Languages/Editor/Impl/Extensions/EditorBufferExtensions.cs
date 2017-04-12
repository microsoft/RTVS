// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor {
    public static class EditorBufferExtensions {
        /// <summary>
        /// Retrieves service from the service container attached to the buffer
        /// </summary>
        public static T GetService<T>(this IEditorBuffer editorBuffer) where T : class
            => editorBuffer.Services.GetService<T>();

        public static void RemoveService<T>(this IEditorBuffer editorBuffer) where T : class
            => editorBuffer.Services.RemoveService<T>();

        /// <summary>
        /// Tries to locate document by a text buffer. 
        /// In trivial case document is attached to the buffer as a service.
        /// </summary>
        public static T GetDocument<T>(this IEditorBuffer editorBuffer) where T : class, IEditorDocument
            => editorBuffer.GetService<T>();
    }
}
