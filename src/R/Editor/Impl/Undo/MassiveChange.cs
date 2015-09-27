using System;
using System.Diagnostics;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.R.Editor.Undo
{
    internal sealed class MassiveChange : IDisposable
    {
        private ITextUndoTransaction _transaction;
        private ITextBuffer _textBuffer;
 
        public MassiveChange(ITextView textView, ITextBuffer textBuffer, string description)
        {
            _textBuffer = textBuffer;

            var undoManagerProvider = EditorShell.Current.ExportProvider.GetExport<ITextBufferUndoManagerProvider>().Value;
            var undoManager = undoManagerProvider.GetTextBufferUndoManager(textView.TextBuffer);

            ITextUndoTransaction innerTransaction = undoManager.TextBufferUndoHistory.CreateTransaction(description);
            _transaction = new TextUndoTransactionThatRollsBackProperly(innerTransaction);

            _transaction.AddUndo(new StartMassiveChangeUndoUnit(_textBuffer));

            IREditorDocument document = REditorDocument.FromTextBuffer(_textBuffer);
            Debug.Assert(document != null);

            document.BeginMassiveChange();
        }

        public void Dispose()
        {
            IREditorDocument document = REditorDocument.FromTextBuffer(_textBuffer);
            Debug.Assert(document != null);

            bool changed = document.EndMassiveChange();
            if (!changed)
            {
                _transaction.Cancel();
            }
            else
            {
                _transaction.AddUndo(new EndMassiveChangeUndoUnit(_textBuffer));
                _transaction.Complete();
            }

            _transaction.Dispose();
        }
    }

    internal class StartMassiveChangeUndoUnit : TextUndoPrimitiveBase
    {
        public StartMassiveChangeUndoUnit(ITextBuffer textBuffer)
            : base(textBuffer)
        {
        }

        public override void Do()
        {
            IREditorDocument document = REditorDocument.TryFromTextBuffer(TextBuffer);
            if (document != null)
                document.BeginMassiveChange();
        }

        public override void Undo()
        {
            IREditorDocument document = REditorDocument.TryFromTextBuffer(TextBuffer);
            if (document != null)
                document.EndMassiveChange();
        }
    }

    internal class EndMassiveChangeUndoUnit : TextUndoPrimitiveBase
    {
        public EndMassiveChangeUndoUnit(ITextBuffer textBuffer)
            : base(textBuffer)
        {
        }

        public override void Do()
        {
            var document = REditorDocument.TryFromTextBuffer(TextBuffer);
            if (document != null)
                document.EndMassiveChange();
        }

        public override void Undo()
        {
            var document = REditorDocument.TryFromTextBuffer(TextBuffer);
            if (document != null)
                document.BeginMassiveChange();
        }
    }
}
