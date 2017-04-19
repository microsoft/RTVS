// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.BraceMatch;
using Microsoft.R.Editor.RData.ContentTypes;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.RData.BraceMatch {
    [Export(typeof(IViewTaggerProvider))]
    [ContentType(RdContentTypeDefinition.ContentType)]
    [TagType(typeof(TextMarkerTag))]
    internal sealed class RdBraceHighlightProvider : BraceHighlightProvider {
        [ImportingConstructor]
        public RdBraceHighlightProvider(ICoreShell shell) : base(shell) { }
    }
}
