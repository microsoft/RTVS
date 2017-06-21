// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Preview.Commands {
    internal abstract class RunChunkCommandBase : ViewCommand {
        protected IRMarkdownEditorSettings Settings { get; }
        private volatile bool _running;

        protected RunChunkCommandBase(ITextView textView, IServiceContainer services, int id) :
            base(textView, new CommandId(MdPackageCommandId.MdCmdSetGuid, id), false) {
            Settings = services.GetService<IRMarkdownEditorSettings>();
        }

        public override CommandStatus Status(Guid group, int id) {
            if (_running) {
                return CommandStatus.Supported;
            }
            if (!TextView.TextBuffer.ContentType.TypeName.EqualsOrdinal(MdProjectionContentTypeDefinition.ContentType)) {
                return CommandStatus.Invisible;
            }
            if(!TextView.IsCaretInRCode()) {
                return CommandStatus.Invisible;
            }
            return Settings.AutomaticSync ? CommandStatus.Supported : CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid @group, int id, object inputArg, ref object outputArg) {
            if (!_running) {
                _running = true;
                ExecuteAsync().ContinueWith(t => _running = false);
            }
            return CommandResult.Executed;
        }

        protected abstract Task ExecuteAsync();
    }
}
