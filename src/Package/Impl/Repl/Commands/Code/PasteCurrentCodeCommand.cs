// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    class PasteCurrentCodeCommand : RExecuteCommand {

        public PasteCurrentCodeCommand(ITextView textView, IRInteractiveWorkflowVisual interactiveWorkflow) :
            base(textView, interactiveWorkflow, new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdPasteReplCmd)) {
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var window = InteractiveWorkflow.ActiveWindow;
            if (window == null) {
                return CommandResult.Disabled;
            }
            
            string text = GetText(window);

            if (text != null) {
                var curBuffer = window.CurrentLanguageBuffer;
                curBuffer.Insert(curBuffer.CurrentSnapshot.Length, text);

                // Caret is where the user clicked, move it to the end of the buffer
                var curSnapshot = TextView.TextBuffer.CurrentSnapshot;
                TextView.Caret.MoveTo(new SnapshotPoint(curSnapshot, curSnapshot.Length));
                TextView.Caret.EnsureVisible();
                return CommandResult.Executed;
            }

            return CommandResult.Disabled;
        }
    }
}
