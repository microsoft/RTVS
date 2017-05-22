// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.Languages.Editor.Text {
    public static class TextViewExtensions {
        public static IEditorView ToEditorView(this ITextView textView) => EditorView.FromTextView(textView);

        /// <summary>
        /// Retrieves service manager attached to the text view
        /// </summary>
        public static IServiceManager Services(this ITextView textView) {
            var view = textView.ToEditorView();
            return view?.Services;
        }

        /// <summary>
        /// Retrieves service from the service container attached to the view
        /// </summary>
        public static T GetService<T>(this ITextView textView) where T : class => textView.Services()?.GetService<T>();

        /// <summary>
        /// Adds service to this instance of the view
        /// </summary>
        public static void AddService<T>(this ITextView textView, T service) where T : class => textView.Services().AddService(service);

        /// <summary>
        /// Removes service from this instance of the view
        /// </summary>
        public static void RemoveService(this ITextView textView, object service) => textView.Services()?.RemoveService(service);

        /// <summary>
        /// Determines caret position in the provided text buffer of a certain content type.
        /// For example, maps caret position from view to one of the source buffers
        /// of the top-level view buffer. If provided buffe is null, the view buffer is used.
        /// </summary>
        public static SnapshotPoint? GetCaretPosition(this ITextView textView, ITextBuffer textBuffer = null)
            => textView.GetCaretPosition(textBuffer?.ContentType.TypeName);

        public static SnapshotPoint? GetCaretPosition(this ITextView textView, IEditorBuffer editorBuffer = null)
            => textView.GetCaretPosition(editorBuffer?.As<ITextBuffer>());

        /// <summary>
        /// Determines caret position in the text buffer of a certain content type.
        /// For example, maps caret position from view to one of the source buffers
        /// of the top-level view buffer. If content type is null, the view buffer is used.
        /// </summary>
        public static SnapshotPoint? GetCaretPosition(this ITextView textView, string contentType = null) {
            try {
                var caretPosition = textView.Caret.Position.BufferPosition;
                if (string.IsNullOrEmpty(contentType) || textView.TextBuffer.ContentType.TypeName.EqualsIgnoreCase(contentType)) {
                    return caretPosition;
                }

                if (textView.TextBuffer is IProjectionBuffer pb) {
                    return pb.MapDown(caretPosition, contentType);
                }
            } catch (ArgumentException) { }
            return null;
        }

        public static bool IsStatementCompletionWindowActive(this ITextView textView, IServiceContainer services) {
            var result = false;
            if (textView != null) {
                var completionBroker = services.GetService<ICompletionBroker>();
                result = completionBroker.IsCompletionActive(textView);
            }
            return result;
        }

        public static SnapshotPoint? MapUpToView(this ITextView textView, SnapshotPoint position) {
            if (textView.BufferGraph == null) {
                // Unit test case
                return position;
            }
            return textView.BufferGraph.MapUpToBuffer(
                position,
                PointTrackingMode.Positive,
                PositionAffinity.Successor,
                textView.TextBuffer
             );
        }
    }
}
