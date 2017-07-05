// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.History.Implementation {
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(RHistoryContentTypeDefinition.ContentType)]
    [TextViewRole(RHistoryWindowVisualComponent.TextViewRole)]
    internal sealed class HistorySelectionTextAdornmentFactory : IWpfTextViewCreationListener {
        [Export(typeof(AdornmentLayerDefinition))]
        [Name(nameof(HistorySelectionTextAdornment))]
        [Order(Before = PredefinedAdornmentLayers.Outlining)]
        [TextViewRole(RHistoryWindowVisualComponent.TextViewRole)]
        public AdornmentLayerDefinition HistorySelectionTextAdornmentLayer { get; set; }

        private readonly IEditorFormatMapService _editorFormatMapService;
        private readonly IRHistoryProvider _historyProvider;

        [ImportingConstructor]
        public HistorySelectionTextAdornmentFactory(IEditorFormatMapService editorFormatMapService, ICoreShell shell) {
            _editorFormatMapService = editorFormatMapService;
            _historyProvider = shell.Services.GetService<IRHistoryProvider>();
        }

        public void TextViewCreated(IWpfTextView textView) {
            textView.Properties.GetOrCreateSingletonProperty(() => new HistorySelectionTextAdornment(textView, _editorFormatMapService, _historyProvider));
        }
    }
}