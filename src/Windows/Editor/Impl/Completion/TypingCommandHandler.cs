// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controller.Commands;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Completion {
    public abstract class TypingCommandHandler : ViewAndBufferCommand {
        private static CommandId[] _commands = {
            new CommandId(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.F1Help),
            new CommandId(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Delete),
            new CommandId(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Escape),
            new CommandId(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Cancel),
            new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.DELETE),
            new CommandId(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Paste),
            new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.PASTE),
            new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.BACKSPACE),
            new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.LEFT),
            new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.RETURN),
            new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.RIGHT),
            new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.TAB),
            new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.TYPECHAR),
            new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.CANCEL),
            new CommandId(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.Cancel),
        };

        protected TypingCommandHandler(ITextView textView)
            : base(textView, _commands, needCheckout: false) {
        }

        public static char GetTypedChar(Guid group, int commandId, object variantIn) {
            char typedChar = '\0';

            if (group == VSConstants.GUID_VSStandardCommandSet97) {
                VSConstants.VSStd97CmdID vsCmdID = (VSConstants.VSStd97CmdID)commandId;

                switch (vsCmdID) {
                    case VSConstants.VSStd97CmdID.Delete:
                        typedChar = '\b';
                        break;

                    case VSConstants.VSStd97CmdID.Escape:
                    case VSConstants.VSStd97CmdID.Cancel:
                        typedChar = (char)0x1B; // ESC
                        break;
                }
            } else if (group == VSConstants.VSStd2K) {
                VSConstants.VSStd2KCmdID vsCmdID = (VSConstants.VSStd2KCmdID)commandId;

                switch (vsCmdID) {
                    case VSConstants.VSStd2KCmdID.DELETE:
                        typedChar = '\b';
                        break;

                    case VSConstants.VSStd2KCmdID.BACKSPACE:
                        typedChar = '\b';
                        break;

                    case VSConstants.VSStd2KCmdID.RETURN:
                        typedChar = '\n';
                        break;

                    case VSConstants.VSStd2KCmdID.TAB:
                        typedChar = '\t';
                        break;

                    case VSConstants.VSStd2KCmdID.Cancel:
                    case VSConstants.VSStd2KCmdID.CANCEL:
                        typedChar = (char)0x1B; // ESC
                        break;

                    case VSConstants.VSStd2KCmdID.TYPECHAR:
                        if (variantIn is char) {
                            typedChar = (char)variantIn;
                        } else {
                            typedChar = (char)(ushort)variantIn;
                        }
                        break;
                }
            }

            return typedChar;
        }

        public override CommandStatus Status(Guid group, int id) {
            if (group == VSConstants.GUID_VSStandardCommandSet97) {
                VSConstants.VSStd97CmdID vsCmdID = (VSConstants.VSStd97CmdID)id;

                switch (vsCmdID) {
                    case VSConstants.VSStd97CmdID.F1Help:
                        if (!string.IsNullOrEmpty(GetHelpTopic())) {
                            return CommandStatus.Supported | CommandStatus.Enabled;
                        }
                        break;
                }
            }

            return CommandStatus.NotSupported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (group == VSConstants.GUID_VSStandardCommandSet97) {
                VSConstants.VSStd97CmdID vsCmdID = (VSConstants.VSStd97CmdID)id;

                switch (vsCmdID) {
                    // If this list of commands gets large, then maybe there's a better way to know when to dismiss
                    case VSConstants.VSStd97CmdID.Paste:
                        DismissAllSessions();
                        break;
                }
            } else if (group == VSConstants.VSStd2K) {
                VSConstants.VSStd2KCmdID vsCmdID = (VSConstants.VSStd2KCmdID)id;

                switch (vsCmdID) {
                    case VSConstants.VSStd2KCmdID.PASTE:
                        DismissAllSessions();
                        break;

                    case VSConstants.VSStd2KCmdID.TAB:
                        // Check if there is selection. If so, TAB will translate to 'indent lines' command
                        // and hence we don't want to trigger intellisense on it.
                        var typedChar = GetTypedChar(group, id, inputArg);
                        if (TextView.Selection.SelectedSpans.Count > 0) {
                            if (TextView.Selection.SelectedSpans[0].Length > 0)
                                typedChar = '\0';
                        }
                        break;
                }
            }

            return HandleCompletion(group, id, inputArg);
        }

        private string GetHelpTopic() {
            // TODO: handle multiple controllers

            var cc = ServiceManager.GetService<CompletionController>(TextView);
            // CompletionController might be null in weird "Open With <different editor>" or diff view scenarios.
            return cc != null ? cc.HelpTopicName : String.Empty;
        }

        private void DismissAllSessions() {
            var completionControllers = ServiceManager.GetAllServices<CompletionController>(TextView);

            foreach (var cc in completionControllers) {
                cc.DismissAllSessions();
            }
        }

        private CommandResult HandleCompletion(Guid group, int id, object inputArg) {
            var completionControllers = ServiceManager.GetAllServices<CompletionController>(TextView);
            char typedChar = GetTypedChar(group, id, inputArg);
            bool handled = false;

            if (typedChar != '\0') {
                foreach (var cc in completionControllers) {
                    if (cc.OnPreTypeChar(typedChar)) {
                        handled = true;
                        break;
                    }
                }
            }

            if (!handled) {
                foreach (var cc in completionControllers) {
                    if (cc.HandleCommand(group, id, inputArg)) {
                        handled = true;
                        break;
                    }
                }
            }

            if (handled) {
                // already handled, stop processing
                return CommandResult.Executed;
            }

            return CommandResult.NotSupported;
        }

        public override void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg) {
            if (result.WasExecuted) {
                char typedChar = '\0';

                if (group == VSConstants.VSStd2K) {
                    // REVIEW: Is the TAB key a trigger for any languages? Maybe this code can be deleted.

                    VSConstants.VSStd2KCmdID vsCmdID = (VSConstants.VSStd2KCmdID)id;
                    typedChar = GetTypedChar(group, id, inputArg);

                    if (vsCmdID == VSConstants.VSStd2KCmdID.TAB) {
                        // Check if there is selection. If so, TAB will translate to 'indent lines' command
                        // and hence we don't want to trigger intellisense on it.
                        if (TextView.Selection.SelectedSpans.Count > 0) {
                            if (TextView.Selection.SelectedSpans[0].Length > 0) {
                                typedChar = '\0';
                            }
                        }
                    }
                }

                if (typedChar != '\0') {
                    OnPostTypeChar(typedChar);
                }
            }
        }

        private void OnPostTypeChar(char typedChar) {
            if (typedChar != '\0') {
                var completionController = this.CompletionController;
                if (completionController != null) {
                    completionController.OnPostTypeChar(typedChar);
                    completionController.FilterCompletionSession();
                }
            }
        }

        protected abstract CompletionController CompletionController { get; }
    }
}
