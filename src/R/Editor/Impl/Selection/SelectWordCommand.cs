// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.R.Components.Controller;
using Microsoft.R.Editor.Navigation.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Selection {
    public sealed class SelectWordCommand : ViewCommand {
        public SelectWordCommand(ITextView textView, ITextBuffer textBuffer) :
            base(textView, typeof(VSConstants.VSStd2KCmdID).GUID, (int)VSConstants.VSStd2KCmdID.SELECTCURRENTWORD, needCheckout: false) {
        }

        public override CommandStatus Status(Guid group, int id) {
            return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (!TextView.Caret.InVirtualSpace) {
                int caretPosition = TextView.Caret.Position.BufferPosition.Position;
                var snapshot = TextView.TextBuffer.CurrentSnapshot;
                Span? spanToSelect = RTextStructure.GetWordSpan(snapshot, caretPosition);
                if (spanToSelect.HasValue && spanToSelect.Value.Length > 0) {
                    TextView.Selection.Select(new SnapshotSpan(snapshot, spanToSelect.Value), isReversed: false);
                    TextView.Caret.MoveTo(new SnapshotPoint(snapshot, spanToSelect.Value.End));
                    return CommandResult.Executed;
                }
            }
            return CommandResult.NotSupported;
        }
    }
}
