// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    internal class FormatSelectionCommand : EditingCommand {
        private readonly ITextBuffer _textBuffer;

        internal FormatSelectionCommand(ITextView textView, ITextBuffer textBuffer, IServiceContainer services)
            : base(textView, services, new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.FORMATSELECTION)) {
            _textBuffer = textBuffer;
        }

        #region ICommand
        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var selectionSpan = TextView.Selection.StreamSelectionSpan.SnapshotSpan;
            var rSpans = TextView.BufferGraph.MapDownToFirstMatch(
                selectionSpan,
                SpanTrackingMode.EdgeInclusive,
                snapshot => snapshot.TextBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType)
            );

            var formatter = new RangeFormatter(Services);
            foreach (var spanToFormat in rSpans) {
                formatter.FormatRange(TextView.ToEditorView(), spanToFormat.Snapshot.TextBuffer.ToEditorBuffer(),
                                           new TextRange(spanToFormat.Start.Position, spanToFormat.Length));
            }
            return new CommandResult(CommandStatus.Supported, 0);
        }

        public override CommandStatus Status(Guid group, int id)
            => TextView.Selection.Mode == TextSelectionMode.Box ? CommandStatus.NotSupported : CommandStatus.SupportedAndEnabled;
        #endregion
    }
}
