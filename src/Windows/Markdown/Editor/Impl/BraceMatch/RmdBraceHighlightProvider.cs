// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.BraceMatch;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.BraceMatch {
    [Export(typeof(IViewTaggerProvider))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    [TagType(typeof(TextMarkerTag))]
    internal sealed class RmdBraceHighlightProvider : BraceHighlightProvider {
        [ImportingConstructor]
        public RmdBraceHighlightProvider(ICoreShell shell) : base(shell) {}
    }
}
