// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.DragDrop;

namespace Microsoft.VisualStudio.R.Package.Sql.DragDrop {
    /// <summary>
    /// Handles file drop on the SQL editor from Solution Explorer
    /// </summary>
    internal sealed class DropHandler : IDropHandler {
        private readonly IWpfTextView _wpfTextView;

        public DropHandler(IWpfTextView wpfTextView) {
            _wpfTextView = wpfTextView;
        }

        #region IDropHandler
        public DragDropPointerEffects HandleDataDropped(DragDropInfo dragDropInfo) {
            HandleDrop(dragDropInfo);
            return DragDropPointerEffects.None;
        }
        public void HandleDragCanceled() { }
        public DragDropPointerEffects HandleDragStarted(DragDropInfo dragDropInfo) => DragDropPointerEffects.Copy | DragDropPointerEffects.Track;
        public DragDropPointerEffects HandleDraggingOver(DragDropInfo dragDropInfo) => DragDropPointerEffects.Copy | DragDropPointerEffects.Track;
        public bool IsDropEnabled(DragDropInfo dragDropInfo) => true;
        #endregion

        private void HandleDrop(DragDropInfo dragDropInfo) {
            var dataObject = dragDropInfo.Data;

            var text = dataObject.GetPlainText();
            var line = _wpfTextView.TextViewLines.GetTextViewLineContainingYCoordinate(dragDropInfo.Location.Y);
            line = line ?? _wpfTextView.TextViewLines.LastVisibleLine;
            if (line == null) {
                return;
            }

            var bufferPosition = line.GetBufferPositionFromXCoordinate(dragDropInfo.Location.X);
            bufferPosition = bufferPosition ?? line.End;

            var textBuffer = _wpfTextView.TextBuffer;
            var dropPosition = bufferPosition.Value;

            if (text.StartsWithOrdinal(Environment.NewLine) && Whitespace.IsNewLineBeforePosition(new TextProvider(textBuffer.CurrentSnapshot), dropPosition)) {
                text = text.TrimStart();
            }
            textBuffer.Replace(new Span(dropPosition, 0), text);
            if (_wpfTextView.Selection != null) {
                _wpfTextView.Caret.MoveTo(_wpfTextView.Selection.End);
            }
        }
    }
}
