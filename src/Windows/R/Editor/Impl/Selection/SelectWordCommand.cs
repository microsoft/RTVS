// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Editor.Navigation.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Selection {
    public sealed class SelectWordCommand : ViewCommand {
        private readonly ITextBuffer _textBuffer;

        public SelectWordCommand(ITextView textView, ITextBuffer textBuffer) :
            base(textView, typeof(VSConstants.VSStd2KCmdID).GUID, (int)VSConstants.VSStd2KCmdID.SELECTCURRENTWORD, needCheckout: false) {
            _textBuffer = textBuffer;
        }

        public override CommandStatus Status(Guid group, int id) {
            return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (!TextView.Caret.InVirtualSpace) {
                var point = TextView.MapDownToBuffer(TextView.Caret.Position.BufferPosition, _textBuffer);
                if (point.HasValue) {
                    var snapshot = point.Value.Snapshot;
                    var spanToSelect = RTextStructure.GetWordSpan(snapshot, point.Value);
                    if (spanToSelect.HasValue && spanToSelect.Value.Length > 0) {
                        var start = TextView.MapUpToView(snapshot, spanToSelect.Value.Start);
                        var end = TextView.MapUpToView(snapshot, spanToSelect.Value.End);
                        if (start.HasValue && end.HasValue) {
                            TextView.Selection.Select(new SnapshotSpan(TextView.TextBuffer.CurrentSnapshot, Span.FromBounds(start.Value, end.Value)), isReversed: false);
                            TextView.Caret.MoveTo(new SnapshotPoint(TextView.TextBuffer.CurrentSnapshot, spanToSelect.Value.End));
                            return CommandResult.Executed;
                        }
                    }
                }
            }
            return CommandResult.NotSupported;
        }
    }
}
