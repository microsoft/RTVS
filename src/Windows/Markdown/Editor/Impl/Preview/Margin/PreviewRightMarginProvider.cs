// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Preview.Margin {
    // Based on https://github.com/madskristensen/MarkdownEditor/blob/master/src/Margin/BrowserMarginProvider.cs

    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name("RMarkdownRightPreviewPane")]
    [Order(After = PredefinedMarginNames.RightControl)]
    [MarginContainer(PredefinedMarginNames.Right)]
    [ContentType(MdContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Debuggable)] // This is to prevent the margin from loading in the diff view
    internal sealed class PreviewRightMarginProvider : PreviewMarginProviderBase {
        [ImportingConstructor]
        public PreviewRightMarginProvider(ICoreShell coreShell) : 
            base(coreShell.Services, RMarkdownPreviewPosition.Right) { }
    }
}