// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;
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

        private readonly IClassificationTypeRegistryService _classificationTypeRegistry;
        private readonly IClassificationFormatMapService _classificationFormatMap;
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public RCodeBackgroundTextAdornmentFactory(IClassificationTypeRegistryService ctrs, IClassificationFormatMapService cfm, ICoreShell coreShell) {
            _classificationTypeRegistry = ctrs;
            _classificationFormatMap = cfm;
            _coreShell = coreShell;
        }

        public void TextViewCreated(IWpfTextView textView) {
            textView.Properties.GetOrCreateSingletonProperty(() => 
                new CodeBackgroundTextAdornment(textView, _classificationFormatMap, _classificationTypeRegistry, _coreShell));
        }
    }
}