// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Selection;
using Microsoft.R.Editor.Undo;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    internal class FormatDocumentCommand : EditingCommand {
        internal FormatDocumentCommand(ITextView textView, ITextBuffer textBuffer, ICoreShell shell)
            : base(textView, shell, new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.FORMATDOCUMENT)) {
            TargetBuffer = textBuffer;
        }

        public virtual ITextBuffer TargetBuffer { get; }

        #region ICommand
        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            string originalText = TargetBuffer.CurrentSnapshot.GetText();
            string formattedText = string.Empty;
            var formatter = new RFormatter(Shell.GetService<IREditorSettings>().FormatOptions);

            try {
                formattedText = formatter.Format(originalText);
            } catch (Exception ex) {
                Debug.Assert(false, "Formatter exception: ", ex.Message);
            }

            if (!string.IsNullOrEmpty(formattedText) && !string.Equals(formattedText, originalText, StringComparison.Ordinal)) {
                var selectionTracker = new RSelectionTracker(TextView, TargetBuffer, new TextRange(0, TargetBuffer.CurrentSnapshot.Length));
                selectionTracker.StartTracking(automaticTracking: false);

                try {
                    using (var massiveChange = new MassiveChange(TextView, TargetBuffer, Shell, Resources.FormatDocument)) {
                        IREditorDocument document = REditorDocument.TryFromTextBuffer(TargetBuffer);
                        if (document != null) {
                            document.EditorTree.Invalidate();
                        }

                        var caretPosition = TextView.Caret.Position.BufferPosition;
                        var viewPortLeft = TextView.ViewportLeft;

                        RTokenizer tokenizer = new RTokenizer();
                        string oldText = TargetBuffer.CurrentSnapshot.GetText();
                        IReadOnlyTextRangeCollection<RToken> oldTokens = tokenizer.Tokenize(oldText);
                        IReadOnlyTextRangeCollection<RToken> newTokens = tokenizer.Tokenize(formattedText);

#if DEBUG
                        //if (oldTokens.Count != newTokens.Count) {
                        //    for (int i = 0; i < Math.Min(oldTokens.Count, newTokens.Count); i++) {
                        //        if (oldTokens[i].TokenType != newTokens[i].TokenType) {
                        //            Debug.Assert(false, Invariant($"Token type difference at {i}"));
                        //            break;
                        //        } else if (oldTokens[i].Length != newTokens[i].Length) {
                        //            Debug.Assert(false, Invariant($"token length difference at {i}"));
                        //            break;
                        //        }
                        //    }
                        //}
#endif
                        IncrementalTextChangeApplication.ApplyChangeByTokens(
                            TargetBuffer,
                            new TextStream(oldText), new TextStream(formattedText),
                            oldTokens, newTokens,
                            TextRange.FromBounds(0, oldText.Length),
                            Resources.FormatDocument, selectionTracker, Shell);
                    }
                } finally {
                    selectionTracker.EndTracking();
                }
                return new CommandResult(CommandStatus.Supported, 0);
            }
            return CommandResult.NotSupported;
        }

        public override CommandStatus Status(Guid group, int id) {
            return CommandStatus.SupportedAndEnabled;
        }
        #endregion
    }
}
