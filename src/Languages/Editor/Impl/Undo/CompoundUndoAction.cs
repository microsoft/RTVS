using System.Diagnostics;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.Languages.Editor.Undo
{
    /// <summary>
    /// Opens and closes a compound undo action in Visual Studio for a given text buffer
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "ICompoundUndoAction.Close is used instead")]
    public class CompoundUndoAction : ICompoundUndoAction, ICompoundUndoActionOptions
    {
        private ITextBufferUndoManager _undoManager;
        private ITextUndoTransaction _undoTransaction;
        private IEditorOperations _editorOperations;
        private bool _undoAfterClose;
        private bool _addRollbackOnCancel;

        public CompoundUndoAction(ITextView textView, ITextBuffer textBuffer, bool addRollbackOnCancel)
        {
            IEditorOperationsFactoryService operationsService = EditorShell.ExportProvider.GetExport<IEditorOperationsFactoryService>().Value;
            ITextBufferUndoManagerProvider undoProvider = EditorShell.ExportProvider.GetExport<ITextBufferUndoManagerProvider>().Value;

            _editorOperations = operationsService.GetEditorOperations(textView);
            _undoManager = undoProvider.GetTextBufferUndoManager(_editorOperations.TextView.TextBuffer);
            _addRollbackOnCancel = addRollbackOnCancel;
        }

        public void Open(string name)
        {
            Debug.Assert(_undoTransaction == null);

            if (_undoTransaction == null)
            {
                _undoTransaction = _undoManager.TextBufferUndoHistory.CreateTransaction(name);

                if (_addRollbackOnCancel)
                {
                    // Some hosts (*cough* VS *cough*) don't properly implement ITextUndoTransaction such
                    //   that their Cancel operation doesn't rollback the already performed actions.
                    //   In those scenarios, we'll use our own rollback mechanism (unabashedly copied
                    //   from Roslyn)
                    _undoTransaction = new TextUndoTransactionThatRollsBackProperly(_undoTransaction);
                }

                _editorOperations.AddBeforeTextBufferChangePrimitive();
            }
        }

        public void Close(bool discardChanges)
        {
            if (_undoTransaction != null)
            {
                if (discardChanges)
                {
                    _undoTransaction.Cancel();
                }
                else
                {
                    _editorOperations.AddAfterTextBufferChangePrimitive();
                    _undoTransaction.Complete();

                    if (_undoAfterClose)
                    {
                        _undoManager.TextBufferUndoHistory.Undo(1);
                    }
                }
            }
        }

        public void SetMergeDirections(bool mergePrevious, bool mergeNext)
        {
            Debug.Assert(_undoTransaction != null);

            if (_undoTransaction != null)
            {
                if (mergePrevious || mergeNext)
                {
                    _undoTransaction.MergePolicy = new MergeUndoActionPolicy(
                        _undoTransaction.Description, mergePrevious, mergeNext, addedTextChangePrimitives: true);
                }
                else
                {
                    _undoTransaction.MergePolicy = null;
                }
            }
        }

        public void SetUndoAfterClose(bool undoAfterClose)
        {
            _undoAfterClose = undoAfterClose;
        }
    }
}
