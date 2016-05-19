// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.Languages.Editor.Services;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Utility;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Commands {
    internal sealed class RunRChunkCommand : ViewCommand {
        private readonly IActiveWpfTextViewTracker _activeTextViewTracker;
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly bool _echo;

        public RunRChunkCommand(ITextView textView, IRInteractiveWorkflow interactiveWorkflow, IActiveWpfTextViewTracker activeTextViewTracker)
            : base(textView, new CommandId(MdPackageCommandId.MdCmdSetGuid, MdPackageCommandId.icmdRunRChunk), false) {
            _interactiveWorkflow = interactiveWorkflow;
            _activeTextViewTracker = activeTextViewTracker;
        }

        public override CommandStatus Status(Guid group, int id) {
            return IsInRCode() ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (!TextView.Caret.InVirtualSpace) {
                var caretPosition = TextView.Caret.Position.BufferPosition;
                var handler = ServiceManager.GetService<IContainedLanguageHandler>(TextView.TextBuffer);
                var codeRange = handler?.GetCodeBlockOfLocation(TextView, caretPosition);

            }
            return CommandResult.Executed;
        }

        private bool IsInRCode() {
            var caret = REditorDocument.MapCaretPositionFromView(TextView);
            return caret.HasValue;
        }
    }
}
