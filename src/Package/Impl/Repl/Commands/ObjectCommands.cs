// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands
{
   

    public sealed class SendToReplObjectSummary : ViewCommand
    {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private ITextView _textView;
        public SendToReplObjectSummary(ITextView textView, IRInteractiveWorkflow interactiveWorkflow) :
            base(textView, new CommandId(RGuidList.RCmdSetGuid, (int)RPackageCommandId.icmdObjectSummary), false)
        {
            _interactiveWorkflow = interactiveWorkflow;
            _textView = textView;
        }

        public override CommandStatus Status(Guid group, int id)
        {
            return (TextView.Selection.Mode == TextSelectionMode.Stream) ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            SendToReplCommand _sendToReplCommand = new SendToReplCommand(_textView, _interactiveWorkflow, "summary(", ")");
            var CommandResult = _sendToReplCommand.Invoke(group, id, inputArg, ref outputArg);
            return CommandResult.Executed;
        }
    }

    public sealed class SendToReplObjectHead : ViewCommand
    {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private ITextView _textView;
        public SendToReplObjectHead(ITextView textView, IRInteractiveWorkflow interactiveWorkflow) :
            base(textView, new CommandId(RGuidList.RCmdSetGuid, (int)RPackageCommandId.icmdObjectHead), false)
        {
            _interactiveWorkflow = interactiveWorkflow;
            _textView = textView;
        }

        public override CommandStatus Status(Guid group, int id)
        {
            return (TextView.Selection.Mode == TextSelectionMode.Stream) ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            SendToReplCommand _sendToReplCommand = new SendToReplCommand(_textView, _interactiveWorkflow, "head(", ")");
            var CommandResult = _sendToReplCommand.Invoke(group, id, inputArg, ref outputArg);
            return CommandResult.Executed;
        }
    }

    public sealed class SendToReplObjectDim : ViewCommand
    {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private ITextView _textView;
        public SendToReplObjectDim(ITextView textView, IRInteractiveWorkflow interactiveWorkflow) :
            base(textView, new CommandId(RGuidList.RCmdSetGuid, (int)RPackageCommandId.icmdObjectDim), false)
        {
            _interactiveWorkflow = interactiveWorkflow;
            _textView = textView;
        }

        public override CommandStatus Status(Guid group, int id)
        {
            return (TextView.Selection.Mode == TextSelectionMode.Stream) ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            SendToReplCommand _sendToReplCommand = new SendToReplCommand(_textView, _interactiveWorkflow, "dim(", ")");
            var CommandResult = _sendToReplCommand.Invoke(group, id, inputArg, ref outputArg);
            return CommandResult.Executed;
        }
    }

    public sealed class SendToReplObjectNames : ViewCommand
    {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private ITextView _textView;
        public SendToReplObjectNames(ITextView textView, IRInteractiveWorkflow interactiveWorkflow) :
            base(textView, new CommandId(RGuidList.RCmdSetGuid, (int)RPackageCommandId.icmdObjectNames), false)
        {
            _interactiveWorkflow = interactiveWorkflow;
            _textView = textView;
        }

        public override CommandStatus Status(Guid group, int id)
        {
            return (TextView.Selection.Mode == TextSelectionMode.Stream) ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            SendToReplCommand _sendToReplCommand = new SendToReplCommand(_textView, _interactiveWorkflow, "names(", ")");
            var CommandResult = _sendToReplCommand.Invoke(group, id, inputArg, ref outputArg);
            return CommandResult.Executed;
        }
    }




}

