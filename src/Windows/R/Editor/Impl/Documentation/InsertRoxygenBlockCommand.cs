// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controller.Commands;
using Microsoft.R.Components.Controller;
using Microsoft.R.Editor.Commands;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Completion.Documentation {
    public sealed class InsertRoxygenBlockCommand : ViewCommand {
        private readonly ITextBuffer _textBuffer;

        public InsertRoxygenBlockCommand(ITextView textView, ITextBuffer textBuffer) :
            base(textView, REditorCommands.REditorCmdSetGuid, REditorCommands.icmdInsertRoxygenBlock, needCheckout: true) {
            _textBuffer = textBuffer;
        }

        public override CommandStatus Status(Guid group, int id) {
            return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            SnapshotPoint? point = REditorDocument.MapCaretPositionFromView(TextView);
            if (point.HasValue) {
                var document = REditorDocument.FromTextBuffer(_textBuffer);
                document.EditorTree.EnsureTreeReady();
                if (RoxygenBlock.TryInsertBlock(_textBuffer, document.EditorTree.AstRoot, point.Value)) {
                    return CommandResult.Executed;
                }
            }
            return CommandResult.NotSupported;
        }
    }
}
