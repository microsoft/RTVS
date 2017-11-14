// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Languages.Editor.Text {
    public static class EditorBufferExtensions {
        /// <summary>
        /// Retrieves service from the service container attached to the buffer
        /// </summary>
        public static T GetService<T>(this IEditorBuffer editorBuffer) where T : class 
            => editorBuffer.Services.GetService<T>();

        /// <summary>
        /// Adds service to this instance of the text buffer
        /// </summary>
        public static void AddService(this IEditorBuffer editorBuffer, object service) 
            => editorBuffer.Services.AddService(service);

        /// <summary>
        /// Removes service from this instance of the text buffer
        /// </summary>
        public static void RemoveService(this IEditorBuffer editorBuffer, object service) 
            => editorBuffer.Services.RemoveService(service);
    }
}
