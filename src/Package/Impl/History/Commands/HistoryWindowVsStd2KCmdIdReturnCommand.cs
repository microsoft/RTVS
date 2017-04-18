// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows.Input;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class HistoryWindowVsStd2KCmdIdReturnCommand : ViewCommand {
        private readonly ICommandTarget _sendToReplCommand;
        private readonly ICommandTarget _sendToSourceCommand;

        public HistoryWindowVsStd2KCmdIdReturnCommand(ITextView textView, ICommandTarget sendToReplCommand, ICommandTarget sendToSourceCommand) 
            : base(textView, VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.RETURN, false) {
            _sendToReplCommand = sendToReplCommand;
            _sendToSourceCommand = sendToSourceCommand;
        }

        public override CommandStatus Status(Guid guid, int id) {
            switch (Keyboard.Modifiers) {
                case ModifierKeys.None:
                    return _sendToReplCommand.Status(guid, id);
                case ModifierKeys.Shift:
                    return _sendToSourceCommand.Status(guid, id);
                default:
                    return CommandStatus.NotSupported;
            }
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            switch (Keyboard.Modifiers) {
                case ModifierKeys.None:
                    return _sendToReplCommand.Invoke(group, id, inputArg, ref outputArg);
                case ModifierKeys.Shift:
                    return _sendToSourceCommand.Invoke(group, id, inputArg, ref outputArg);
                default:
                    return CommandResult.NotSupported;
            }
        }
    }
}