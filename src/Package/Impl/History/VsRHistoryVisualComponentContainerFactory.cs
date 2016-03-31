// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.History;
using Microsoft.R.Components.View;
using Microsoft.VisualStudio.R.Package.Windows;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History {
    [Export(typeof(IRHistoryVisualComponentContainerFactory))]
    internal class VsRHistoryVisualComponentContainerFactory : ToolWindowPaneFactory<HistoryWindowPane>, IRHistoryVisualComponentContainerFactory {
        private readonly IRHistoryProvider _historyProvider;
        private readonly ITextEditorFactoryService _textEditorFactory;

        [ImportingConstructor]
        public VsRHistoryVisualComponentContainerFactory(ITextEditorFactoryService textEditorFactory, IRHistoryProvider historyProvider) {
            _textEditorFactory = textEditorFactory;
            _historyProvider = historyProvider;
        }

        public IVisualComponentContainer<IRHistoryWindowVisualComponent> GetOrCreate(ITextBuffer historyTextBuffer, int instanceId) {
            return GetOrCreate(instanceId, i => new HistoryWindowPane(historyTextBuffer, _historyProvider, _textEditorFactory));
        }
    }
}
