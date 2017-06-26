// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Document;
using Microsoft.Markdown.Editor.Utility;
using Microsoft.R.Components.InteractiveWorkflow;
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
            if (!TextView.TextBuffer.ContentType.TypeName.EqualsOrdinal(MdProjectionContentTypeDefinition.ContentType)) {
                return CommandStatus.Invisible;
            }
            return TextView.IsCaretInRCode() ? CommandStatus.SupportedAndEnabled : CommandStatus.Invisible;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (!TextView.Caret.InVirtualSpace) {
                var caretPosition = TextView.Caret.Position.BufferPosition;
                var document = TextView.TextBuffer.GetEditorDocument<IMdEditorDocument>();
                var handler = document?.ContainedLanguageHandler;
                var codeRange = handler?.GetCodeBlockOfLocation(caretPosition);
                if (codeRange != null) {
                    var code = TextView.TextBuffer.CurrentSnapshot.GetText(new Span(codeRange.Start, codeRange.Length));
                    code = MarkdownUtility.GetRContentFromMarkdownCodeBlock(code).Trim();
                    if (!string.IsNullOrWhiteSpace(code)) {
                        try {
                            _interactiveWorkflow.Operations.ExecuteExpression(code);
                        } catch (RException) { } catch (OperationCanceledException) { }
                    }
                }
            }
            return CommandResult.Executed;
        }
    }
}
