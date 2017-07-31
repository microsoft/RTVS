// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.Undo;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using TextRange = Microsoft.Languages.Core.Text.TextRange;

namespace Microsoft.Markdown.Editor.Formatting {
    internal class FormatDocumentCommand : EditingCommand {
        internal FormatDocumentCommand(ITextView textView, ITextBuffer textBuffer, IServiceContainer services)
            : base(textView, services, new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.FORMATDOCUMENT)) {
            TargetBuffer = textBuffer;
        }

        public virtual ITextBuffer TargetBuffer { get; }

        #region ICommand

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var clh = TargetBuffer.GetService<IContainedLanguageHandler>();
            var o = new object();
            var originalSelection = TextView.Selection.StreamSelectionSpan;

            using (new MassiveChange(TextView, TargetBuffer, Services, Resources.FormatDocument)) {
                foreach (var block in clh.LanguageBlocks) {
                    var paramsRange = GetParameterBlockRange(TargetBuffer.CurrentSnapshot, block);
                    var formatRange = TextRange.FromBounds(paramsRange.End, block.End);

                    var formatSpan = new SnapshotSpan(TextView.TextBuffer.CurrentSnapshot, formatRange.ToSpan());
                    TextView.Selection.Select(formatSpan, false);

                    var cmdTarget = clh.GetCommandTargetOfLocation(TextView, formatRange.Start);
                    cmdTarget.Invoke(VSConstants.VSStd2K, (int) VSConstants.VSStd2KCmdID.FORMATSELECTION, null, ref o);
                }

                if(clh.LanguageBlocks.Count > 0) {
                    var newSelection = originalSelection.TranslateTo(TextView.TextBuffer.CurrentSnapshot, SpanTrackingMode.EdgePositive);
                    TextView.Selection.Select(newSelection.SnapshotSpan, false);
                    TextView.Caret.MoveTo(newSelection.SnapshotSpan.Start);
                }
            }
            return CommandResult.Executed;
        }

        public override CommandStatus Status(Guid group, int id) => CommandStatus.SupportedAndEnabled;
        #endregion

        private ITextRange GetParameterBlockRange(ITextSnapshot snapshot, ITextRange block) {
            var content = snapshot.GetText(block.ToSpan());

            var start = content.IndexOf('{');
            if(start < 0) {
                return TextRange.FromBounds(block.Start, block.Start);
            }

            var bc = new BraceCounter<char>('{', '}');
            var end = start;
            bc.CountBrace(content[end]);
            while (bc.Count > 0 && end < content.Length) {
                end++;
                bc.CountBrace(content[end]);
            }

            return TextRange.FromBounds(block.Start + start, block.Start + end + 1);
        }
    }
}
