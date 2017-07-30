// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor {
    public static class EditorViewExtensions {
        /// <summary>
        /// Retrieves service from the service container attached to the view
        /// </summary>
        public static T GetService<T>(this IEditorView editorView) where T : class => editorView.Services.GetService<T>();
        
        /// <summary>
        /// Adds service to this instance of the view
        /// </summary>
        public static void AddService<T>(this IEditorView editorView, object service) => editorView.Services.AddService(service);
        
        /// <summary>
        /// Removes service from the instance of the view
        /// </summary>
        public static void RemoveService(this IEditorView editorView, object service) => editorView.Services.RemoveService(service);
    }
}
