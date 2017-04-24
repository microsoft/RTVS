// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.R.Components.Test.Fakes.Undo {
    /// <summary>
    /// This class is to make it easy to catch new undo/redo operations while a delegated primitive
    /// is in progress--it is called from DelegatedUndoPrimitive.Undo and .Redo with the IDispose
    /// using pattern to set up the history to send operations our way.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class CatchOperationsFromHistoryForDelegatedPrimitive : IDisposable {
        private readonly UndoHistoryImpl _history;
        private readonly DelegatedUndoPrimitiveImpl _primitive;

        public CatchOperationsFromHistoryForDelegatedPrimitive(UndoHistoryImpl history, DelegatedUndoPrimitiveImpl primitive, DelegatedUndoPrimitiveState state) {
            _history = history;
            _primitive = primitive;

            primitive.State = state;
            history.ForwardToUndoOperation(primitive);
        }

        public void Dispose() {
            _history.EndForwardToUndoOperation(_primitive);
            _primitive.State = DelegatedUndoPrimitiveState.Inactive;
            GC.SuppressFinalize(this);
        }
    }
}
