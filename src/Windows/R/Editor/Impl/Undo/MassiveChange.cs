// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.R.Editor.Undo {
    internal sealed class MassiveChange : IDisposable {
        private readonly ITextUndoTransaction _transaction;
        private readonly ITextBuffer _textBuffer;

        public MassiveChange(ITextView textView, ITextBuffer textBuffer, IServiceContainer services, string description) {
            _textBuffer = textBuffer;

            var undoManagerProvider = services.GetService<ITextBufferUndoManagerProvider>();
            var undoManager = undoManagerProvider.GetTextBufferUndoManager(textView.TextBuffer);

            var innerTransaction = undoManager.TextBufferUndoHistory.CreateTransaction(description);
            _transaction = new TextUndoTransactionThatRollsBackProperly(innerTransaction);

            _transaction.AddUndo(new StartMassiveChangeUndoUnit(_textBuffer));

            var document = _textBuffer.GetEditorDocument<IREditorDocument>();
            document?.BeginMassiveChange();
        }

        public void Dispose() {
            var document = _textBuffer.GetEditorDocument<IREditorDocument>();
            var changed = true;

            if (document != null) {
                changed = document.EndMassiveChange();
            }

            if (!changed) {
                _transaction.Cancel();
            } else {
                _transaction.AddUndo(new EndMassiveChangeUndoUnit(_textBuffer));
                _transaction.Complete();
            }

            _transaction.Dispose();
        }
    }

    internal class StartMassiveChangeUndoUnit : TextUndoPrimitiveBase {
        public StartMassiveChangeUndoUnit(ITextBuffer textBuffer)
            : base(textBuffer) {
        }

        public override void Do() {
            var document = TextBuffer.GetEditorDocument<IREditorDocument>();
            document?.BeginMassiveChange();
        }

        public override void Undo() {
            var document = TextBuffer.GetEditorDocument<IREditorDocument>();
            document?.EndMassiveChange();
        }
    }
    internal class EndMassiveChangeUndoUnit : TextUndoPrimitiveBase {
        public EndMassiveChangeUndoUnit(ITextBuffer textBuffer)
            : base(textBuffer) {
        }

        public override void Do() {
            var document = TextBuffer.GetEditorDocument<IREditorDocument>();
            document?.EndMassiveChange();
        }

        public override void Undo() {
            var document = TextBuffer.GetEditorDocument<IREditorDocument>();
            document?.BeginMassiveChange();
        }
    }
}

