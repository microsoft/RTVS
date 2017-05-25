// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Comments {
    internal sealed class UncommentCommand : EditingCommand {
        internal UncommentCommand(ITextView textView, ITextBuffer textBuffer, IServiceContainer services)
            : base(textView, services, new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK)) {
        }

        #region ICommand
        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var selectionSpan = TextView.Selection.StreamSelectionSpan.SnapshotSpan;

            RCommenter.UncommentBlock(TextView.ToEditorView(), TextView.TextBuffer.ToEditorBuffer(),
                new TextRange(selectionSpan.Start.Position, selectionSpan.Length), Services.GetService<IEditorSupport>());

            return CommandResult.Executed;
        }

        public override CommandStatus Status(Guid group, int id)=> CommandStatus.SupportedAndEnabled;
        #endregion
    }
}
