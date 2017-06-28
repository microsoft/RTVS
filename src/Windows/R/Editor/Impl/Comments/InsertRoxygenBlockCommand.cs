// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.Commands;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Roxygen;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Comments {
    public sealed class InsertRoxygenBlockCommand : ViewCommand {
        private readonly ITextBuffer _textBuffer;

        public InsertRoxygenBlockCommand(ITextView textView, ITextBuffer textBuffer) :
            base(textView, REditorCommands.REditorCmdSetGuid, REditorCommands.icmdInsertRoxygenBlock, needCheckout: true) {
            _textBuffer = textBuffer;
        }

        public override CommandStatus Status(Guid group, int id) => CommandStatus.SupportedAndEnabled;

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var point = TextView.GetCaretPosition(_textBuffer);
            if (point.HasValue) {
                var document = _textBuffer.GetEditorDocument<IREditorDocument>();
                document.EditorTree.EnsureTreeReady();
                if (RoxygenBlock.TryInsertBlock(document.EditorBuffer, document.EditorTree.AstRoot, point.Value)) {
                    return CommandResult.Executed;
                }
            }
            return CommandResult.NotSupported;
        }
    }
}
