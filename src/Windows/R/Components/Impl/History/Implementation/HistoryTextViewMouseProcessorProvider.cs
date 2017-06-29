// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.History.Implementation {
    [Export(typeof(IMouseProcessorProvider))]
    [Name(nameof(HistoryWindowPaneMouseProcessor))]
    [Order(Before = "WordSelection")]
    [ContentType(RHistoryContentTypeDefinition.ContentType)]
    [TextViewRole(RHistoryWindowVisualComponent.TextViewRole)]
    internal sealed class HistoryWindowPaneMouseProcessorProvider : IMouseProcessorProvider {
        private readonly IRHistoryProvider _historyProvider;

        [ImportingConstructor]
        public HistoryWindowPaneMouseProcessorProvider(ICoreShell coreShell) {
            _historyProvider = coreShell.Services.GetService<IRHistoryProvider>();
        }

        public IMouseProcessor GetAssociatedProcessor(IWpfTextView wpfTextView) {
            return wpfTextView.Properties.GetOrCreateSingletonProperty(() => new HistoryWindowPaneMouseProcessor(_historyProvider.GetAssociatedRHistory(wpfTextView)));
        }
    }
}