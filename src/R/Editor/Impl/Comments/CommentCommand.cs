// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Controller.Commands;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Comments {
    internal class CommentCommand : EditingCommand {
        internal CommentCommand(ITextView textView, ITextBuffer textBuffer, ICoreShell shell)
            : base(textView, shell, new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.COMMENT_BLOCK)) {
        }

        #region ICommand
        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            SnapshotSpan selectionSpan = TextView.Selection.StreamSelectionSpan.SnapshotSpan;

            RCommenter.CommentBlock(TextView, TextView.TextBuffer, new TextRange(selectionSpan.Start.Position, selectionSpan.Length), Shell);
            return CommandResult.Executed;
        }

        public override CommandStatus Status(Guid group, int id) => CommandStatus.SupportedAndEnabled;
        #endregion
    }
}
