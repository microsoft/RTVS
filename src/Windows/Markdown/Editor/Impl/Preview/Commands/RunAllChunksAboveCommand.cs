// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Preview.Commands {
    internal sealed class RunAllChunksAboveCommand : ViewCommand {
        private readonly IRMarkdownEditorSettings _settings;

        public RunAllChunksAboveCommand(ITextView textView, IServiceContainer services) :
            base(textView, new CommandId(MdPackageCommandId.MdCmdSetGuid, MdPackageCommandId.icmdRunAllChunksAbove), false) {
            _settings = services.GetService<IRMarkdownEditorSettings>();
        }

        public override CommandStatus Status(Guid group, int id) {
            if (!TextView.TextBuffer.ContentType.TypeName.EqualsOrdinal(MdProjectionContentTypeDefinition.ContentType)) {
                return CommandStatus.Invisible;
            }
            if(!TextView.IsCaretInRCode()) {
                return CommandStatus.Invisible;
            }
            return _settings.AutomaticSync ? CommandStatus.Supported : CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid @group, int id, object inputArg, ref object outputArg) {
            TextView.GetService<IMarkdownPreview>()?.RunAllChunksAbove();
            return CommandResult.Executed;
        }
    }
}
