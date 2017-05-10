// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.Languages.Editor.Formatting;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Selection;
using Microsoft.R.Editor.Undo;
using Microsoft.R.Editor.Windows;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    internal class FormatDocumentCommand : EditingCommand {
        internal FormatDocumentCommand(ITextView textView, ITextBuffer textBuffer, IServiceContainer services)
            : base(textView, services, new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.FORMATDOCUMENT)) {
            TargetBuffer = textBuffer;
        }

        public virtual ITextBuffer TargetBuffer { get; }

        #region ICommand
        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var originalText = TargetBuffer.CurrentSnapshot.GetText();
            var formattedText = string.Empty;
            var formatter = new RFormatter(Services.GetService<IREditorSettings>().FormatOptions);

            try {
                formattedText = formatter.Format(originalText);
            } catch (Exception ex) {
                Debug.Assert(false, "Formatter exception: ", ex.Message);
            }

            if (!string.IsNullOrEmpty(formattedText) && !string.Equals(formattedText, originalText, StringComparison.Ordinal)) {
                var selectionTracker = new RSelectionTracker(TextView, TargetBuffer, new TextRange(0, TargetBuffer.CurrentSnapshot.Length));
                selectionTracker.StartTracking(automaticTracking: false);

                try {
                    using (var massiveChange = new MassiveChange(TextView, TargetBuffer, Services, Windows_Resources.FormatDocument)) {
                        var document = TargetBuffer.GetEditorDocument<IREditorDocument>();
                        document?.EditorTree?.Invalidate();

                        var tokenizer = new RTokenizer();
                        var oldText = TargetBuffer.CurrentSnapshot.GetText();
                        var oldTokens = tokenizer.Tokenize(oldText);
                        var newTokens = tokenizer.Tokenize(formattedText);

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
                        var wsChangeHandler = Services.GetService<IIncrementalWhitespaceChangeHandler>();
                        wsChangeHandler.ApplyChange(
                            TargetBuffer.ToEditorBuffer(),
                            new TextStream(oldText), new TextStream(formattedText),
                            oldTokens, newTokens,
                            TextRange.FromBounds(0, oldText.Length),
                            Windows_Resources.FormatDocument, selectionTracker);
                    }
                } finally {
                    selectionTracker.EndTracking();
                }
                return new CommandResult(CommandStatus.Supported, 0);
            }
            return CommandResult.NotSupported;
        }

        public override CommandStatus Status(Guid group, int id) => CommandStatus.SupportedAndEnabled;
        #endregion
    }
}
