// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.R.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.Commands {
    internal sealed class InsertRCodeBlock : ViewCommand {
        private readonly IREditorSettings _settings;
        public InsertRCodeBlock(ITextView textView, IServiceContainer services)
            : base(textView, new CommandId(MdPackageCommandId.MdCmdSetGuid, MdPackageCommandId.icmdInsertRCodeBlock), false) {
            _settings = services.GetService<IREditorSettings>();
        }

        public override CommandStatus Status(Guid group, int id) {
            if (!TextView.TextBuffer.ContentType.TypeName.EqualsOrdinal(MdProjectionContentTypeDefinition.ContentType)) {
                return CommandStatus.Invisible;
            }
            return TextView.IsCaretInRCode() ? CommandStatus.Supported : CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            const string fragment = @"
```{r}

```
";
            var caretPosition = TextView.Caret.Position.BufferPosition;
            var tb = TextView.TextBuffer;
            var lineNum = tb.CurrentSnapshot.GetLineNumberFromPosition(caretPosition);

            TextView.TextBuffer.Insert(caretPosition, fragment);
            var newLine = tb.CurrentSnapshot.GetLineFromLineNumber(lineNum + 2);
            TextView.Caret.MoveTo(new VirtualSnapshotPoint(newLine, _settings.TabSize));

            return CommandResult.Executed;
        }
    }
}
