// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Navigation {
    public sealed class PeekDefinitionCommand : ViewCommand {
        private ITextBuffer _textBuffer;

        public PeekDefinitionCommand(ITextView textView, ITextBuffer textBuffer) :
           base(textView, new CommandId(typeof(VSConstants.VSStd97CmdID).GUID,
                (int)VSConstants.VSStd97CmdID.GotoDefn), needCheckout: false) {
            _textBuffer = textBuffer;
        }

        public override CommandStatus Status(Guid group, int id) {
            return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            return CommandResult.Executed;
        }
    }
}
