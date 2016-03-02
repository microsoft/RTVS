// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.BraceMatch;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.BraceMatch
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [TagType(typeof(TextMarkerTag))]
    internal sealed class RBraceHighlightProvider : BraceHighlightProvider
    {
    }
}
