// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    class RExecuteCommand : ViewCommand {
        protected IRInteractiveWorkflowVisual InteractiveWorkflow { get; }

        public RExecuteCommand(ITextView textView, IRInteractiveWorkflowVisual interactiveWorkflow, CommandId id) : base(textView, id, false) {
            InteractiveWorkflow = interactiveWorkflow;
        }

        public override CommandStatus Status(Guid group, int id) {
            var window = InteractiveWorkflow.ActiveWindow;
            if (window == null ||
                InteractiveWorkflow.ActiveWindow.IsRunning) {
                return CommandStatus.NotSupported;
            }

            return GetText(window) != null ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;
        }

        protected string GetText(IInteractiveWindowVisualComponent window) {
            string text = null;
            if (TextView.Selection.IsActive && !TextView.Selection.IsEmpty) {
                // If the user has a selection, we want to use it..
                StringBuilder tmpText = new StringBuilder();
                foreach (var span in TextView.Selection.SelectedSpans) {
                    foreach (var code in window.TextView.MapDownToR(span)) {
                        // we never include the current input buffer in the appended code
                        if (span.Snapshot.TextBuffer != window.CurrentLanguageBuffer) {
                            tmpText.Append(code.GetText());
                        }
                    }
                }

                if (tmpText.Length > 0) {
                    text = tmpText.ToString();
                }
            } else {
                // Otherwise use the selection that the caret is currently in, if it's not the
                // current language buffer
                var langBuffer = window.TextView.MapDownToR(window.TextView.Caret.Position.BufferPosition);
                if (langBuffer != null && langBuffer.Value.Snapshot.TextBuffer != window.CurrentLanguageBuffer) {
                    text = TrimText(langBuffer.Value.Snapshot.GetText());
                }
            }

            return text;
        }

        private string TrimText(string text) {
            var newLine = TextView.Options.GetNewLineCharacter();
            if (text.EndsWithOrdinal(newLine)) {
                text = text.Substring(0, text.Length - newLine.Length);
            }

            return text;
        }
    }
}
