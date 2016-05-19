// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.Services;
using Microsoft.Markdown.Editor.Utility;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Document;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Commands {
    internal sealed class RunRChunkCommand : ViewCommand {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;

        public RunRChunkCommand(ITextView textView, IRInteractiveWorkflow interactiveWorkflow)
            : base(textView, new CommandId(MdPackageCommandId.MdCmdSetGuid, MdPackageCommandId.icmdRunRChunk), false) {
            _interactiveWorkflow = interactiveWorkflow;
        }

        public override CommandStatus Status(Guid group, int id) {
            return IsInRCode() ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (!TextView.Caret.InVirtualSpace) {
                var caretPosition = TextView.Caret.Position.BufferPosition;
                var handler = ServiceManager.GetService<IContainedLanguageHandler>(TextView.TextBuffer);
                var codeRange = handler?.GetCodeBlockOfLocation(TextView, caretPosition);
                if (codeRange != null) {
                    var code = TextView.TextBuffer.CurrentSnapshot.GetText(new Span(codeRange.Start, codeRange.Length));
                    code = MarkdownUtility.GetRContentFromMarkdownCodeBlock(code);
                    if (!string.IsNullOrWhiteSpace(code)) {
                        _interactiveWorkflow.RSession.ExecuteAsync(code);
                    }
                }
            }
            return CommandResult.Executed;
        }

        private bool IsInRCode() {
            var caret = REditorDocument.MapCaretPositionFromView(TextView);
            return caret.HasValue;
        }
    }
}
