// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.SuggestedActions;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.Markdown.Editor.Preview;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.SuggestedActions {
    internal sealed class RunChunksAboveSuggestedAction : SuggestedActionBase {
        public RunChunksAboveSuggestedAction(ITextView textView, ITextBuffer textBuffer, int position) :
            base(textView, textBuffer, position, Resources.SuggestedAction_RunAllChunksAbove, KnownMonikers.GoToBottom) { }

        public override void Invoke(CancellationToken cancellationToken) {
            var preview = TextView.GetService<IMarkdownPreview>();
            preview?.RunAllChunksAboveAsync().DoNotWait();
        }

        public override bool TryGetTelemetryId(out Guid telemetryId) {
            telemetryId = MdPackageCommandId.MdCmdSetGuid;
            return true;
        }
    }
}
