// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Formatting;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    class ReplFormatDocumentCommand : FormatDocumentCommand {
        public ReplFormatDocumentCommand(ITextView view, ITextBuffer buffer, IEditorShell editorShell) 
            : base(view, buffer, editorShell) { }

        public override ITextBuffer TargetBuffer => base.TargetBuffer.GetInteractiveWindow().CurrentLanguageBuffer;
    }
}
