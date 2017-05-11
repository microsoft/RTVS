// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Completions {
    public abstract class CompletionCommandHandler : ViewCommand {
        private static CommandId[] _commandIds = {
            new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.SHOWMEMBERLIST),
            new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.COMPLETEWORD),
            new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.PARAMINFO),
            new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.QUICKINFO),
        };

        protected CompletionCommandHandler(ITextView textView) :
            base(textView, _commandIds, false) {}

        public abstract CompletionController CompletionController { get; }

        #region ICommand
        public override CommandStatus Status(Guid group, int id) {
            // CompletionController will be null in weird scenarios, such as "Open With <non-html editor>" or diff view
            if (CompletionController != null && group == VSConstants.VSStd2K) {
                VSConstants.VSStd2KCmdID vsCmdID = (VSConstants.VSStd2KCmdID)id;

                switch (vsCmdID) {
                    case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
                    case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                    case VSConstants.VSStd2KCmdID.PARAMINFO:
                    case VSConstants.VSStd2KCmdID.QUICKINFO:
                        return CommandStatus.SupportedAndEnabled;
                }
            }
            return CommandStatus.NotSupported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            // CompletionController will be null in weird scenarios, such as "Open With <non-html editor>" or diff view
            if (CompletionController != null && group == VSConstants.VSStd2K) {
                VSConstants.VSStd2KCmdID vsCmdID = (VSConstants.VSStd2KCmdID)id;

                switch (vsCmdID) {
                    case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
                        bool filterList = (inputArg is bool) && (bool)inputArg;
                        CompletionController.OnShowMemberList(filterList);
                        return CommandResult.Executed;

                    case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                        CompletionController.OnCompleteWord();
                        return CommandResult.Executed;

                    case VSConstants.VSStd2KCmdID.PARAMINFO:
                        CompletionController.OnShowSignatureHelp();
                        return CommandResult.Executed;

                    case VSConstants.VSStd2KCmdID.QUICKINFO:
                        CompletionController.OnShowQuickInfo();
                        return CommandResult.Executed;
                }
            }
            return CommandResult.NotSupported;
        }
        #endregion
    }
}
