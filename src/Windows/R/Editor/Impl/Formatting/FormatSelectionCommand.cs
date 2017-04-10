// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Controller.Commands;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Controller;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    internal class FormatSelectionCommand : EditingCommand {
        ITextBuffer _textBuffer;

        internal FormatSelectionCommand(ITextView textView, ITextBuffer textBuffer, ICoreShell shell)
            : base(textView, shell, new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.FORMATSELECTION)) {
            _textBuffer = textBuffer;
        }

        #region ICommand
        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            SnapshotSpan selectionSpan = TextView.Selection.StreamSelectionSpan.SnapshotSpan;
            var rSpans = TextView.BufferGraph.MapDownToFirstMatch(
                selectionSpan,
                SpanTrackingMode.EdgeInclusive,
                snapshot => snapshot.TextBuffer.ContentType.IsOfType(RContentTypeDefinition.ContentType)
            );

            foreach (var spanToFormat in rSpans) {
                RangeFormatter.FormatRange(TextView, spanToFormat.Snapshot.TextBuffer,
                                           new TextRange(spanToFormat.Start.Position, spanToFormat.Length),
                                           Shell.GetService<IREditorSettings>().FormatOptions, Shell);
            }

            return new CommandResult(CommandStatus.Supported, 0);
        }

        public override CommandStatus Status(Guid group, int id)
            => TextView.Selection.Mode == TextSelectionMode.Box ? CommandStatus.NotSupported : CommandStatus.SupportedAndEnabled;
        #endregion
    }
}
