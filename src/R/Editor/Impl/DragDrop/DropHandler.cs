// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Formatting;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.DragDrop;

namespace Microsoft.R.Editor.DragDrop {
    /// <summary>
    /// Handles file drop on the editor. Primarily from Solution Explorer
    /// </summary>
    internal sealed class DropHandler : IDropHandler {
        private readonly IWpfTextView _wpfTextView;
        private readonly IEditorShell _editorShell;
        private readonly IWorkspaceServices _wsps;

        public DropHandler(IWpfTextView wpfTextView, IEditorShell editorShell, IWorkspaceServices wsps) {
            _wpfTextView = wpfTextView;
            _editorShell = editorShell;
            _wsps = wsps;
        }

        #region IDropHandler
        public DragDropPointerEffects HandleDataDropped(DragDropInfo dragDropInfo) {
            var dataObject = dragDropInfo.Data;

            var document = REditorDocument.FindInProjectedBuffers(_wpfTextView.TextBuffer);
            Debug.Assert(document != null, "Text view does not have associated R document.");

            var text = dataObject.GetPlainText(_wsps.ActiveProjectPath);
            var line = _wpfTextView.TextViewLines.GetTextViewLineContainingYCoordinate(dragDropInfo.Location.Y);
            line = line ?? _wpfTextView.TextViewLines.LastVisibleLine;
            if (line == null) {
                return DragDropPointerEffects.None;
            }

            var bufferPosition = line.GetBufferPositionFromXCoordinate(dragDropInfo.Location.X);
            bufferPosition = bufferPosition ?? line.End;

            var textBuffer = _wpfTextView.TextBuffer;
            var dropPosition = bufferPosition.Value;

            if (REditorSettings.FormatOnPaste) {
                _wpfTextView.Caret.MoveTo(dropPosition);
            }

            if (text.StartsWithOrdinal(Environment.NewLine) && Whitespace.IsNewLineBeforePosition(new TextProvider(textBuffer.CurrentSnapshot), dropPosition)) {
                text = text.TrimStart();
            }

            using (var undoAction = EditorShell.Current.CreateCompoundAction(_wpfTextView, textBuffer)) {
                undoAction.Open(Resources.DragDropOperation);
                textBuffer.Replace(new Span(dropPosition, 0), text);

                if (REditorSettings.FormatOnPaste) {
                    RangeFormatter.FormatRange(_wpfTextView, document.TextBuffer, new TextRange(dropPosition, text.Length), REditorSettings.FormatOptions, _editorShell);
                }

                if (_wpfTextView.Selection != null) {
                    _wpfTextView.Caret.MoveTo(_wpfTextView.Selection.End);
                }
                undoAction.Commit();
            }
            return DragDropPointerEffects.None;
        }

        public void HandleDragCanceled() { }
        public DragDropPointerEffects HandleDragStarted(DragDropInfo dragDropInfo) => DragDropPointerEffects.Copy | DragDropPointerEffects.Track;
        public DragDropPointerEffects HandleDraggingOver(DragDropInfo dragDropInfo) => DragDropPointerEffects.Copy | DragDropPointerEffects.Track;
        public bool IsDropEnabled(DragDropInfo dragDropInfo) => true;
        #endregion
    }
}
