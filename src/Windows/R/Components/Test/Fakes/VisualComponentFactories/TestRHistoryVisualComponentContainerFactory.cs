// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.History;
using Microsoft.R.Components.History.Implementation;
using Microsoft.R.Components.View;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.Test.Fakes.VisualComponentFactories {
    [ExcludeFromCodeCoverage]
    [Export(typeof (IRHistoryVisualComponentContainerFactory))]
    internal sealed class TestRHistoryVisualComponentContainerFactory : ContainerFactoryBase<IRHistoryWindowVisualComponent>, IRHistoryVisualComponentContainerFactory {
        private readonly IRHistoryProvider _historyProvider;
        private readonly ITextEditorFactoryService _textEditorFactory;

        [ImportingConstructor]
        public TestRHistoryVisualComponentContainerFactory(ICoreShell coreShell) {
            _historyProvider = coreShell.GetService<IRHistoryProvider>();
            _textEditorFactory = coreShell.GetService<ITextEditorFactoryService>();
        }

        public IVisualComponentContainer<IRHistoryWindowVisualComponent> GetOrCreate(ITextBuffer historyTextBuffer, int instanceId = 0) {
            return GetOrCreate(instanceId, container => new RHistoryWindowVisualComponent(historyTextBuffer, _historyProvider, _textEditorFactory, container));
        }
    }
}