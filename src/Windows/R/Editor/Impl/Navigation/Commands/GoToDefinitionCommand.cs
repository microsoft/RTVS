// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Navigation.Commands {
    public sealed class GoToDefinitionCommand : ViewCommand {
        private readonly ITextBuffer _textBuffer;
        private readonly IObjectViewer _objectViewer;
        private readonly IRSession _session;

        public GoToDefinitionCommand(ITextView textView, ITextBuffer textBuffer, IObjectViewer objectViewer, IRSession session) :
           base(textView, new CommandId(typeof(VSConstants.VSStd97CmdID).GUID,
                (int)VSConstants.VSStd97CmdID.GotoDefn), needCheckout: false) {
            _textBuffer = textBuffer;
            _objectViewer = objectViewer;
            _session = session;
        }

        public override CommandStatus Status(Guid group, int id) {
            return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            string itemName;
            var viewPoint = CodeNavigator.FindCurrentItemDefinition(TextView, _textBuffer, out itemName);
            if (viewPoint.HasValue) {
                TextView.Caret.MoveTo(new SnapshotPoint(TextView.TextBuffer.CurrentSnapshot, viewPoint.Value));
                TextView.Caret.EnsureVisible();
            } else {
                // Try View(item) in case this is internal function
                _objectViewer?.ViewObjectDetails(_session, REnvironments.GlobalEnv, itemName, itemName).DoNotWait();
            }
            return CommandResult.Executed;
        }
    }
}
