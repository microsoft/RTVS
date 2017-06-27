// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.History;
using Microsoft.R.Components.View;
using Microsoft.VisualStudio.R.Package.Windows;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.R.Package.History {
    [Export(typeof(IRHistoryVisualComponentContainerFactory))]
    internal class VsRHistoryVisualComponentContainerFactory : ToolWindowPaneFactory<HistoryWindowPane>, IRHistoryVisualComponentContainerFactory {
        private readonly IRHistoryProvider _historyProvider;
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public VsRHistoryVisualComponentContainerFactory(ICoreShell coreShell) : base(coreShell.Services) {
            _coreShell = coreShell;
            _historyProvider = _coreShell.GetService<IRHistoryProvider>();
        }

        public IVisualComponentContainer<IRHistoryWindowVisualComponent> GetOrCreate(ITextBuffer historyTextBuffer, int instanceId) 
            => GetOrCreate(instanceId, i => new HistoryWindowPane(historyTextBuffer, _historyProvider, _coreShell.Services));
    }
}
