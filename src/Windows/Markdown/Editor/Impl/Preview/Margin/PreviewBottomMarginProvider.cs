// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Preview.Margin {
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name("RMarkdownBottomPreviewPane")]
    [Order(After = PredefinedMarginNames.BottomControl)]
    [MarginContainer(PredefinedMarginNames.Bottom)]
    [ContentType(MdContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Debuggable)] // This is to prevent the margin from loading in the diff view
    internal sealed class PreviewBottomMarginProvider : PreviewMarginProviderBase {
        [ImportingConstructor]
        public PreviewBottomMarginProvider(ICoreShell coreShell) :
            base(coreShell.Services, RMarkdownPreviewPosition.Below) { }
    }
}
