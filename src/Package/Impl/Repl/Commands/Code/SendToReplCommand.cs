// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    public sealed class SendToReplCommand : ViewCommand {
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;

        public SendToReplCommand(ITextView textView, IRInteractiveWorkflowVisual interactiveWorkflow) :
            base(textView, new CommandId(RGuidList.RCmdSetGuid, (int)RPackageCommandId.icmdSendToRepl), false) { 
            _interactiveWorkflow = interactiveWorkflow;
        }

        public override CommandStatus Status(Guid group, int id) {
            return (TextView.Selection.Mode == TextSelectionMode.Stream) ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            var selection = TextView.Selection;
            var snapshot = TextView.TextBuffer.CurrentSnapshot;
            var position = selection.Start.Position;
            var line = snapshot.GetLineFromPosition(position);

            var window = _interactiveWorkflow.ActiveWindow;
            if (window == null) {
                return CommandResult.Disabled;
            }

            string text;
            if (selection.StreamSelectionSpan.Length == 0) {
                text = line.GetText();
            } else {
                text = TextView.Selection.StreamSelectionSpan.GetText();
                line = TextView.Selection.End.Position.GetContainingLine();
            }

            window.Container.Show(focus: false, immediate: false);
            _interactiveWorkflow.Operations.EnqueueExpression(text, true);

            var targetLine = line;
            while (targetLine.LineNumber < snapshot.LineCount - 1) {
                targetLine = snapshot.GetLineFromLineNumber(targetLine.LineNumber + 1);
                // skip over blank lines, unless it's the last line, in which case we want to land on it no matter what
                if (!string.IsNullOrWhiteSpace(targetLine.GetText()) || targetLine.LineNumber == snapshot.LineCount - 1) {
                    TextView.Caret.MoveTo(new SnapshotPoint(snapshot, targetLine.Start));
                    TextView.Caret.EnsureVisible();
                    break;
                }
            }

            // Take focus back if REPL window has stolen it
            if (!TextView.HasAggregateFocus) {
                var adapterService = _interactiveWorkflow.Shell.GetService<IVsEditorAdaptersFactoryService>();
                var tv = adapterService.GetViewAdapter(TextView);
                tv.SendExplicitFocus();
            }
            
            return CommandResult.Executed;
        }
    }
}
