// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.Languages.Editor.Extensions {
    public static class EditorExtensions {
        /// <summary>
        /// Given text view buffer and the content type, locates document 
        /// in the underlying  text buffer graph.
        /// </summary>
        public static T FindInProjectedBuffers<T>(ITextBuffer viewBuffer, string contentType) where T: class, IEditorDocument {
            if (viewBuffer.ContentType.IsOfType(contentType)) {
                return ServiceManager.GetService<T>(viewBuffer);
            }

            T document = null;
            ITextBuffer rBuffer = null;
            var pb = viewBuffer as IProjectionBuffer;
            if (pb != null) {
                rBuffer = pb.SourceBuffers.FirstOrDefault((ITextBuffer tb) => {
                    if (tb.ContentType.IsOfType(contentType)) {
                        document = ServiceManager.GetService<T>(tb);
                        if (document != null) {
                            return true;
                        }
                    }
                    return false;
                });
            }
            return document;
        }

        public static T TryFromTextBuffer<T>(ITextBuffer textBuffer, string contentType) where T: class, IEditorDocument {
            var document = ServiceManager.GetService<T>(textBuffer);
            if (document == null) {
                document = FindInProjectedBuffers<T>(textBuffer, contentType);
                if (document == null) {
                    TextViewData viewData = TextViewConnectionListener.GetTextViewDataForBuffer(textBuffer);
                    if (viewData != null && viewData.LastActiveView != null) {
                        var controller = ViewController.FromTextView(viewData.LastActiveView);
                        if (controller != null && controller.TextBuffer != null) {
                            document = ServiceManager.GetService<T>(controller.TextBuffer);
                        }
                    }
                }
            }
            return document;
        }

        public static ITextView GetFirstView(this IEditorDocument document) {
            return document.TextBuffer.GetFirstView();
        }

        public static ITextView GetFirstView(this ITextBuffer textBuffer) {
            return TextViewConnectionListener.GetFirstViewForBuffer(textBuffer);
        }
    }
}
