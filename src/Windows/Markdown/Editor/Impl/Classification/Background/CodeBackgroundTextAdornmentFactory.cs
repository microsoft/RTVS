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
    internal sealed class CodeBackgroundTextAdornmentFactory : IWpfTextViewCreationListener {
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "MEF")]
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("CodeBackgroundTextAdornment")]
        [Order(Before = PredefinedAdornmentLayers.Outlining)]
        [TextViewRole(PredefinedTextViewRoles.Document)]
        public AdornmentLayerDefinition EditorAdornmentLayer { get; set; }

        private readonly IClassificationTypeRegistryService _classificationTypeRegistry;
        private readonly IClassificationFormatMapService _classificationFormatMap;

        [ImportingConstructor]
        public CodeBackgroundTextAdornmentFactory(IClassificationTypeRegistryService ctrs, IClassificationFormatMapService cfm) {
            _classificationTypeRegistry = ctrs;
            _classificationFormatMap = cfm;
        }

        public void TextViewCreated(IWpfTextView textView) {
            textView.Properties.GetOrCreateSingletonProperty(() => 
                new CodeBackgroundTextAdornment(textView, _classificationFormatMap, _classificationTypeRegistry));
        }
    }
}