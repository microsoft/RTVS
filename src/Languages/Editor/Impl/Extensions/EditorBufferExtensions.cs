// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Document;

namespace Microsoft.Languages.Editor.Text {
    public static class EditorBufferExtensions {
        /// <summary>
        /// Retrieves service from the service container attached to the buffer
        /// </summary>
        public static T GetService<T>(this IEditorBuffer editorBuffer) where T : class => editorBuffer.Services.GetService<T>();

        /// <summary>
        /// Adds service to this instance of the text buffer
        /// </summary>
        public static void AddService<T>(this IEditorBuffer editorBuffer, T service) where T : class => editorBuffer.Services.AddService<T>(service);

        /// <summary>
        /// Removes service from this instance of the text buffer
        /// </summary>
        public static void RemoveService<T>(this IEditorBuffer editorBuffer) where T : class => editorBuffer.Services.RemoveService<T>();

        /// <summary>
        /// Tries to locate document by a text buffer. 
        /// In trivial case document is attached to the buffer as a service.
        /// </summary>
        public static T GetDocument<T>(this IEditorBuffer editorBuffer) where T : class, IEditorDocument
            => editorBuffer.GetService<T>();
    }
}
