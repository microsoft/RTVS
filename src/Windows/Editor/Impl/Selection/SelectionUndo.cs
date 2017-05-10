// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.Undo;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.Languages.Editor.Selection {
    /// <summary>
    /// An undo unit that helps preserve caret position and relative
    /// screen position over text buffer when user performs undo/redo
    /// operations on a change that potentially changes caret position
    /// and/or view port position. For example, in automatic formatting.
    /// </summary>
    public sealed class SelectionUndo : IDisposable {
        private readonly ISelectionTracker _selectionTracker;
        private readonly ITextBuffer _textBuffer;
        private readonly ITextUndoTransaction _transaction;

        public SelectionUndo(ISelectionTracker selectionTracker, ITextBufferUndoManagerProvider undoManagerProvider, string transactionName, bool automaticTracking) {
            _selectionTracker = selectionTracker;
            _textBuffer = selectionTracker.EditorView.As<ITextView>().TextBuffer;
            var undoManager = undoManagerProvider.GetTextBufferUndoManager(_textBuffer);

            var innerTransaction = undoManager.TextBufferUndoHistory.CreateTransaction(transactionName);
            _transaction = new TextUndoTransactionThatRollsBackProperly(innerTransaction);
            _transaction.AddUndo(new StartSelectionTrackingUndoUnit(selectionTracker, _textBuffer));

            _selectionTracker.StartTracking(automaticTracking);
        }

        public void Dispose() {
            _selectionTracker.EndTracking();
            _transaction.AddUndo(new EndSelectionTrackingUndoUnit(_selectionTracker, _textBuffer));
            _transaction.Complete();
            _transaction.Dispose();
        }
    }

    /// <summary>
    /// 'Forward' ('do') action selection undo
    /// </summary>
    internal class StartSelectionTrackingUndoUnit : TextUndoPrimitiveBase {
        private readonly ISelectionTracker _selectionTracker;

        public StartSelectionTrackingUndoUnit(ISelectionTracker selectionTracker, ITextBuffer textBuffer)
            : base(textBuffer) {
            _selectionTracker = selectionTracker;
        }

        public override void Undo() => _selectionTracker.MoveToBeforeChanges();
    }

    /// <summary>
    /// Reverse ('undo') selection unit.
    /// </summary>
    internal class EndSelectionTrackingUndoUnit : TextUndoPrimitiveBase {
        private readonly ISelectionTracker _selectionTracker;

        public EndSelectionTrackingUndoUnit(ISelectionTracker selectionTracker, ITextBuffer textBuffer)
            : base(textBuffer) {
            _selectionTracker = selectionTracker;
        }

        public override void Do() => _selectionTracker.MoveToAfterChanges();
    }
}
