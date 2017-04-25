// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Controllers.Views;
using Microsoft.Languages.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.Languages.Editor.Text {
    public static class TextBufferExtensions {
        public static IEditorBuffer ToEditorBuffer(this ITextBuffer textBuffer)
            => textBuffer.Properties.TryGetProperty(typeof(IEditorBuffer), out IEditorBuffer buffer) ? buffer : null;

        /// <summary>
        /// Retrieves service manager attached to the text buffer
        /// </summary>
        public static IServiceManager Services(this ITextBuffer textBuffer) {
            var editorBuffer = textBuffer.ToEditorBuffer();
            Check.InvalidOperation(() => editorBuffer != null);
            return editorBuffer.Services;
        }

        /// <summary>
        /// Retrieves service from the service container attached to the text buffer
        /// </summary>
        public static T GetService<T>(this ITextBuffer textBuffer) where T : class => textBuffer.Services().GetService<T>();

        /// <summary>
        /// Adds service to the service container attached to the text buffer
        /// </summary>
        public static void AddService<T>(this ITextBuffer textBuffer, T service) where T : class => textBuffer.Services().AddService(service);

        /// <summary>
        /// Removes service from the service container attached to the text buffer
        /// </summary>
        public static void RemoveService(this ITextBuffer textBuffer, object service) => textBuffer.Services().RemoveService(service);

        /// <summary>
        /// Attempts to locate associated editor document. Implementation depends on the platform.
        /// </summary>
        /// <typeparam name="T">Type of the document to locate</typeparam>
        public static T GetEditorDocument<T>(this ITextBuffer textBuffer) where T : class, IEditorDocument => textBuffer.ToEditorBuffer()?.GetEditorDocument<T>();

        /// <summary>
        /// Retrieves last active view, if any, for the text buffer instance.
        /// Text buffer is not guaranteed to have a view.
        /// </summary>
        public static ITextView CurrentTextView(this ITextBuffer textBuffer) {
            ITextView textView = null;
            if (textBuffer != null) {
                var textViewData = TextViewConnectionListener.GetTextViewDataForBuffer(textBuffer);
                textView = textViewData?.LastActiveView;
            }
            return textView;
        }

        public static IEnumerable<ITextBuffer> GetContributingBuffers(this ITextBuffer textBuffer) {
            List<ITextBuffer> allBuffers = new List<ITextBuffer>();

            allBuffers.Add(textBuffer);
            for (int i = 0; i < allBuffers.Count; i++) {
                var currentBuffer = allBuffers[i] as IProjectionBuffer;
                if (currentBuffer != null) {
                    foreach (ITextBuffer sourceBuffer in currentBuffer.SourceBuffers) {
                        if (!allBuffers.Contains(sourceBuffer))
                            allBuffers.Add(sourceBuffer);
                    }
                }
            }

            return allBuffers;
        }

        /// <summary>
        /// Retrieves full path to the file opened in the text buffer.
        /// Not all text buffers have files associated with them,
        /// </summary>
        public static string GetFileName(this ITextBuffer textBuffer) {
            string path = string.Empty;
            ITextDocument document = textBuffer.GetTextDocument();
            if (document != null && document.FilePath != null) {
                path = document.FilePath;
            }
            return path;
        }

        public static ITextDocument GetTextDocument(this ITextBuffer textBuffer) {
            var searchBuffers = textBuffer.GetContributingBuffers();
            return searchBuffers
                .Select(buffer => {
                    ITextDocument document = null;
                    buffer.Properties.TryGetProperty(typeof(ITextDocument), out document);
                    return document;
                })
                .Where(d => d != null)
                .FirstOrDefault();
        }

        /// <summary>
        /// Converts stream buffer position to line and column.
        /// </summary>
        /// <returns>True if position was successfully converted</returns>
        public static bool GetLineColumnFromPosition(this ITextBuffer textBuffer, int position, out int line, out int column) {
            line = 0;
            column = 0;

            try {
                var textLine = textBuffer.CurrentSnapshot.GetLineFromPosition(position);
                if (textLine != null) {
                    line = textLine.LineNumber;
                    column = position - textLine.Start.Position;
                    return true;
                }
            } catch (Exception) { }
            return false;
        }

        /// <summary>
        /// Converts line and column positions to a stream buffer position.
        /// </summary>
        /// <returns>Stream position or null if conversion failed</returns>
        public static int? GetPositionFromLineColumn(this ITextBuffer textBuffer, int line, int column)
            => textBuffer.CurrentSnapshot.GetPositionFromLineColumn(line, column);

        public static int? GetPositionFromLineColumn(this ITextSnapshot snapshot, int line, int column) {
            if ((line >= 0) && (line < snapshot.LineCount)) {
                var textLine = snapshot.GetLineFromLineNumber(line);

                // Non-strict equality below, because caret can be position *after*
                // the last character of the line. So for line of length 1 both 
                // column 0 and column 1 are valid caret locations.
                if (column <= textLine.Length) {
                    return textLine.Start + column;
                }
            }

            return null;
        }

        public static bool IsSignatureHelpBuffer(this ITextBuffer textBuffer)
            => textBuffer.ContentType.TypeName.EndsWithIgnoreCase(" Signature Help");

        public static void AddBufferDisposedAction(this ITextBuffer textBuffer, ICoreShell shell, Action<ITextBuffer, ICoreShell> callback) {
            if (shell != null) {
                var textDocumentFactoryService = shell.GetService<ITextDocumentFactoryService>();
                ITextDocument textDocument;

                if (textDocumentFactoryService.TryGetTextDocument(textBuffer, out textDocument)) {
                    EventHandler<TextDocumentEventArgs> onDocumentDisposed = null;
                    onDocumentDisposed = (object sender, TextDocumentEventArgs eventArgs) => {
                        if (eventArgs.TextDocument == textDocument) {
                            textDocumentFactoryService.TextDocumentDisposed -= onDocumentDisposed;

                            callback(textBuffer, shell);
                        }
                    };

                    textDocumentFactoryService.TextDocumentDisposed += onDocumentDisposed;
                }
            }
        }

        // Returns spans corresponding to the changes that occurred between startVersion and endVersion
        public static bool GetChangedExtent(this ITextVersion oldVersion, ITextVersion newVersion, out Span? oldSpan, out Span? newSpan) {
            oldSpan = null;
            newSpan = null;

            if (oldVersion.VersionNumber > newVersion.VersionNumber) {
                // They've asked for information about an earlier snapshot, not supported
                Debug.Assert(false);
                return false;
            }

            int newEnd = Int32.MinValue;
            int position = Int32.MaxValue;
            int deltaLen = 0;
            while (oldVersion != newVersion) {
                var changes = oldVersion.Changes;
                if (changes.Count > 0) {
                    var firstChange = changes[0];
                    var lastChange = changes[changes.Count - 1];
                    int changeDeltaLen = lastChange.NewEnd - lastChange.OldEnd;

                    deltaLen += changeDeltaLen;

                    position = Math.Min(position, firstChange.NewPosition);

                    if (newEnd < lastChange.OldEnd) {
                        newEnd = lastChange.NewEnd;
                    } else {
                        newEnd += changeDeltaLen;
                    }
                }
                oldVersion = oldVersion.Next;
            }

            if (newEnd < position) {
                // There weren't any changes between the versions, return a null TextChangeExtent
                return false;
            }

            int oldEnd = newEnd - deltaLen;
            oldSpan = Span.FromBounds(position, oldEnd);
            newSpan = Span.FromBounds(position, newEnd);

            return true;
        }

        /// <summary>
        /// Given top-level text buffer (i.e. view buffer) maps location down
        /// the buffer graph to the project buffer of the specified content type, if any.
        /// </summary>
        public static SnapshotPoint? MapDown(this ITextBuffer viewBuffer, SnapshotPoint viewPoint, string contentTypeName) {
            SnapshotPoint? point = null;
            if (viewBuffer.ContentType.TypeName.EqualsOrdinal(contentTypeName)) {
                point = viewPoint;
            } else {
                var pb = viewBuffer as IProjectionBuffer;
                var bg = viewBuffer.GetBufferGraph();
                if (pb != null && bg != null) {
                    // There may be multiple buffers with the same content type. 
                    // This is normal in the interactive window. Therefore we must look 
                    // for a buffer that CAN map the given point and not just take
                    // the first one from the list.
                    point = pb.SourceBuffers
                            .Where(x => x.ContentType.TypeName.EqualsOrdinal(contentTypeName))
                            .Select((buffer) => bg.MapDownToBuffer(viewPoint, PointTrackingMode.Positive, buffer, PositionAffinity.Successor))
                            .FirstOrDefault(p => p.HasValue);
                }
            }
            return point;
        }

        /// <summary>
        /// Given top-level text buffer (i.e. view buffer) and location in the projected
        /// buffer maps location up the buffer graph from the buffer of the specified
        /// content type to the view buffer.
        /// </summary>
        public static SnapshotPoint? MapUp(this ITextBuffer viewBuffer, SnapshotPoint sourcePoint, string contentTypeName) {
            if (viewBuffer.ContentType.TypeName.EqualsOrdinal(contentTypeName)) {
                return sourcePoint;
            } else {
                var bg = viewBuffer.GetBufferGraph();
                return bg?.MapUpToBuffer(sourcePoint, PointTrackingMode.Positive, PositionAffinity.Successor, viewBuffer);
            }
        }

        /// <summary>
        /// Given top-level text buffer (i.e. view buffer) and location in the projected
        /// buffer maps location up the buffer graph from the buffer of the specified
        /// content type to the view buffer.
        /// </summary>
        public static SnapshotSpan? MapUp(this ITextBuffer viewBuffer, SnapshotSpan sourceSpan, string contentTypeName) {
            if (viewBuffer.ContentType.TypeName.EqualsOrdinal(contentTypeName)) {
                return new SnapshotSpan(viewBuffer.CurrentSnapshot, sourceSpan);
            } else {
                var bg = viewBuffer.GetBufferGraph();
                var sourceBuffer = (viewBuffer as IProjectionBuffer)?.SourceBuffers.FirstOrDefault(x => x.ContentType.TypeName.EqualsOrdinal(contentTypeName));
                var spans = bg?.MapUpToBuffer(sourceSpan, SpanTrackingMode.EdgePositive, viewBuffer);
                return spans?.FirstOrDefault();
            }
        }

        /// <summary>
        /// Given view buffer (top-level buffer in the projection tree) returns buffer graph.
        /// </summary>
        public static IBufferGraph GetBufferGraph(this ITextBuffer viewBuffer) {
            var pb = viewBuffer as IProjectionBuffer;
            return pb?.Properties.PropertyList.FirstOrDefault(x => x.Value is IBufferGraph).Value as IBufferGraph;
        }
    }
}
