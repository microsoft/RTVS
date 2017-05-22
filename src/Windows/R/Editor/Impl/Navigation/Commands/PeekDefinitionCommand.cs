// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Navigation.Commands {
    public sealed class PeekDefinitionCommand : ViewCommand {
        private readonly ITextBuffer _textBuffer;
        private readonly IPeekBroker _peekBroker;

        public PeekDefinitionCommand(ITextView textView, ITextBuffer textBuffer, IPeekBroker peekBroker) :
           base(textView, new CommandId(typeof(VSConstants.VSStd12CmdID).GUID,
                (int)VSConstants.VSStd12CmdID.PeekDefinition), needCheckout: false) {
            _textBuffer = textBuffer;
            _peekBroker = peekBroker;
        }

        public override CommandStatus Status(Guid group, int id) {
            return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _peekBroker.TriggerPeekSession(TextView, PredefinedPeekRelationships.Definitions.Name);
            return CommandResult.Executed;
        }
    }
}
