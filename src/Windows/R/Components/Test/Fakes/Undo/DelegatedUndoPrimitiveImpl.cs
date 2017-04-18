// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.R.Components.Test.Fakes.Undo {
    /// <summary>
    /// This is the implementation of a primitive to support inverse operations, where the user does not supply their own
    /// primitives. Rather, the user calls "AddUndo" on the history and we build the primitive for them.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class DelegatedUndoPrimitiveImpl : ITextUndoPrimitive {
        private readonly Stack<UndoableOperationCurried> _undoOperations;
        private UndoTransactionImpl _parent;
        private readonly UndoHistoryImpl _history;
        private DelegatedUndoPrimitiveState _state;

        public DelegatedUndoPrimitiveState State
        {
            get { return _state; }
            set { _state = value; }
        }

        public DelegatedUndoPrimitiveImpl(UndoHistoryImpl history, UndoTransactionImpl parent, UndoableOperationCurried operationCurried) {
            RedoOperations = new Stack<UndoableOperationCurried>();
            _undoOperations = new Stack<UndoableOperationCurried>();

            _parent = parent;
            _history = history;
            _state = DelegatedUndoPrimitiveState.Inactive;

            _undoOperations.Push(operationCurried);
        }

        public bool CanRedo => RedoOperations.Count > 0;

        public bool CanUndo => _undoOperations.Count > 0;

        /// <summary>
        /// Here, we undo everything in the list of undo operations, and then clear the list. While this is happening, the
        /// History will collect new operations for the redo list and pass them on to us.
        /// </summary>
        public void Undo() {
            using (new CatchOperationsFromHistoryForDelegatedPrimitive(_history, this, DelegatedUndoPrimitiveState.Undoing)) {
                while (_undoOperations.Count > 0) {
                    _undoOperations.Pop()();
                }
            }
        }

        /// <summary>
        /// This is only called for "Redo," not for the original "Do." The action is to redo everything in the list of
        /// redo operations, and then clear the list. While this is happening, the History will collect new operations
        /// for the undo list and pass them on to us.
        /// </summary>
        public void Do() {
            using (new CatchOperationsFromHistoryForDelegatedPrimitive(_history, this, DelegatedUndoPrimitiveState.Redoing)) {
                while (RedoOperations.Count > 0) {
                    RedoOperations.Pop()();
                }
            }
        }

        public ITextUndoTransaction Parent {
            get { return _parent; }
            set { _parent = value as UndoTransactionImpl; }
        }

        /// <summary>
        /// This is called by the UndoHistory implementation when we are mid-undo/mid-redo and
        /// the history receives a new UndoableOperation. The action is then to add that operation
        /// to the inverse list.
        /// </summary>
        /// <param name="operation"></param>
        public void AddOperation(UndoableOperationCurried operation) {
            if (_state == DelegatedUndoPrimitiveState.Redoing) {
                _undoOperations.Push(operation);
            } else if (_state == DelegatedUndoPrimitiveState.Undoing) {
                RedoOperations.Push(operation);
            } else {
                throw new InvalidOperationException("Strings.DelegatedUndoPrimitiveStateDoesNotAllowAdd");
            }
        }

        public bool MergeWithPreviousOnly => true;

        internal Stack<UndoableOperationCurried> RedoOperations { get; }

        public bool CanMerge(ITextUndoPrimitive primitive) {
            return false;
        }

        public ITextUndoPrimitive Merge(ITextUndoPrimitive primitive) {
            throw new InvalidOperationException("Strings.DelegatedUndoPrimitiveCannotMerge");
        }
    }
}