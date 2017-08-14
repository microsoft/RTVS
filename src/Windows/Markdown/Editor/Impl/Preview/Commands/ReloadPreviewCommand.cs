// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Preview.Commands {
    internal sealed class ReloadPreviewCommand : ViewCommand {
        public ReloadPreviewCommand(ITextView textView, IServiceContainer services) :
            base(textView, new CommandId(MdPackageCommandId.MdCmdSetGuid, MdPackageCommandId.icmdReloadPreview), false) { }

        public override CommandStatus Status(Guid group, int id) => CommandStatus.SupportedAndEnabled;

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            TextView.GetService<IMarkdownPreview>()?.Reload();
            return CommandResult.Executed;
        }
    }
}
