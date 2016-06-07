// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Classification.Background {
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class RCodeBackgroundTextAdornmentFactory : IWpfTextViewCreationListener {
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "MEF")]
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("CodeBackgroundTextAdornment")]
        [Order(Before = PredefinedAdornmentLayers.Outlining)]
        [TextViewRole(PredefinedTextViewRoles.Document)]
        public AdornmentLayerDefinition editorAdornmentLayer = null;

        [Import]
        public IClassificationTypeRegistryService ClassificationTypeRegistry { get; set; }

        [Import]
        public IClassificationFormatMapService ClassificationFormatMap { get; set; }

        public void TextViewCreated(IWpfTextView textView) {
            new CodeBackgroundTextAdornment(textView, ClassificationFormatMap, ClassificationTypeRegistry);
        }
    }
}