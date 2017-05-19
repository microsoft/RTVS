// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Testing;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.Languages.Editor.Undo {
    /// <summary>
    /// Opens and closes a compound undo action in Visual Studio for a given text buffer
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "ICompoundUndoAction.Close is used instead")]
    public sealed class CompoundUndoAction : ICompoundUndoAction, ICompoundUndoActionOptions {
        private readonly ITextBufferUndoManager _undoManager;
        private ITextUndoTransaction _undoTransaction;
        private readonly IEditorOperations _editorOperations;
        private readonly bool _addRollbackOnCancel;
        private bool _undoAfterClose;
        private bool _discardChanges = true;

        public CompoundUndoAction(IEditorView editorView, IServiceContainer services, bool addRollbackOnCancel = true) {
            var shell = services.GetService<ICoreShell>();
            if (TestEnvironment.Current == null) {
                var operationsService = services.GetService<IEditorOperationsFactoryService>();
                var undoProvider = services.GetService<ITextBufferUndoManagerProvider>();

                _editorOperations = operationsService.GetEditorOperations(editorView.As<ITextView>());
                _undoManager = undoProvider.GetTextBufferUndoManager(_editorOperations.TextView.TextBuffer);
                _addRollbackOnCancel = addRollbackOnCancel;
            }
        }

        public void Open(string name) {
            Debug.Assert(_undoTransaction == null);

            if (_undoTransaction == null && _undoManager != null && _editorOperations != null) {
                _undoTransaction = _undoManager.TextBufferUndoHistory.CreateTransaction(name);
                if (_addRollbackOnCancel) {
                    // Some hosts (*cough* VS *cough*) don't properly implement ITextUndoTransaction such
                    //   that their Cancel operation doesn't rollback the already performed actions.
                    //   In those scenarios, we'll use our own rollback mechanism (unabashedly copied
                    //   from Roslyn)
                    _undoTransaction = new TextUndoTransactionThatRollsBackProperly(_undoTransaction);
                }

                _editorOperations.AddBeforeTextBufferChangePrimitive();
            }
        }

        /// <summary>
        /// Marks action as successful. Dispose will place the undo unit on the undo stack.
        /// </summary>
        public void Commit() => _discardChanges = false;

        public void Dispose() {
            if (_undoTransaction != null) {
                if (_discardChanges) {
                    _undoTransaction.Cancel();
                } else {
                    _editorOperations.AddAfterTextBufferChangePrimitive();
                    _undoTransaction.Complete();

                    if (_undoAfterClose) {
                        _undoManager.TextBufferUndoHistory.Undo(1);
                    }
                }
            }
        }

        public void SetMergeDirections(bool mergePrevious, bool mergeNext) {
            Debug.Assert(_undoTransaction != null);

            if (_undoTransaction != null) {
                if (mergePrevious || mergeNext) {
                    _undoTransaction.MergePolicy = new MergeUndoActionPolicy(
                        _undoTransaction.Description, mergePrevious, mergeNext, addedTextChangePrimitives: true);
                } else {
                    _undoTransaction.MergePolicy = null;
                }
            }
        }

        public void SetUndoAfterClose(bool undoAfterClose) => _undoAfterClose = undoAfterClose;
    }
}
