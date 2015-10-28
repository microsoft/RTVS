using System;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
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
        private ISelectionTracker _selectionTracker;
        private ITextUndoTransaction _transaction;

        public SelectionUndo(ITextView textView, string transactionName) :
            this(new SelectionTracker(textView), transactionName, true) {
        }

        public SelectionUndo(ITextView textView, string transactionName, bool automaticTracking) :
            this(new SelectionTracker(textView), transactionName, automaticTracking) {
        }

        public SelectionUndo(ISelectionTracker selectionTracker, string transactionName, bool automaticTracking) {
            if (!EditorShell.Current.IsUnitTestEnvironment) {
                _selectionTracker = selectionTracker;

                var undoManagerProvider = EditorShell.Current.ExportProvider.GetExport<ITextBufferUndoManagerProvider>().Value;
                var undoManager = undoManagerProvider.GetTextBufferUndoManager(selectionTracker.TextView.TextBuffer);

                ITextUndoTransaction innerTransaction = undoManager.TextBufferUndoHistory.CreateTransaction(transactionName);
                _transaction = new TextUndoTransactionThatRollsBackProperly(innerTransaction);
                _transaction.AddUndo(new StartSelectionTrackingUndoUnit(selectionTracker));

                _selectionTracker.StartTracking(automaticTracking);
            }
        }

        public void Dispose() {
            if (!EditorShell.Current.IsUnitTestEnvironment) {
                _selectionTracker.EndTracking();

                _transaction.AddUndo(new EndSelectionTrackingUndoUnit(_selectionTracker));

                _transaction.Complete();
                _transaction.Dispose();
            }
        }
    }

    /// <summary>
    /// 'Forward' ('do') action selection undo
    /// </summary>
    internal class StartSelectionTrackingUndoUnit : TextUndoPrimitiveBase {
        private ISelectionTracker _selectionTracker;

        public StartSelectionTrackingUndoUnit(ISelectionTracker selectionTracker)
            : base(selectionTracker.TextView.TextBuffer) {
            _selectionTracker = selectionTracker;
        }

        public override void Undo() {
            _selectionTracker.MoveToBeforeChanges();
        }
    }

    /// <summary>
    /// Reverse ('undo') selection unit.
    /// </summary>
    internal class EndSelectionTrackingUndoUnit : TextUndoPrimitiveBase {
        private ISelectionTracker _selectionTracker;

        public EndSelectionTrackingUndoUnit(ISelectionTracker selectionTracker)
            : base(selectionTracker.TextView.TextBuffer) {
            _selectionTracker = selectionTracker;
        }

        public override void Do() {
            _selectionTracker.MoveToAfterChanges();
        }
    }
}
