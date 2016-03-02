// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.History;
using Microsoft.R.Components.History.Implementation;
using Microsoft.R.Components.Test.Stubs.VisualComponents;
using Microsoft.R.Components.View;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Test.Mocks {
    public class RHistoryVisualComponentContainerFactoryMock : IRHistoryVisualComponentContainerFactory {
        private readonly IRHistoryProvider _historyProvider;
        private readonly ITextEditorFactoryService _textEditorFactory;

        public RHistoryVisualComponentContainerFactoryMock(IRHistoryProvider historyProvider, ITextEditorFactoryService textEditorFactory) {
            _historyProvider = historyProvider;
            _textEditorFactory = textEditorFactory;
        }

        public IVisualComponentContainer<IRHistoryWindowVisualComponent> GetOrCreate(ITextBuffer historyTextBuffer, int instanceId = 0) {
            var container = new VisualComponentContainerStub<IRHistoryWindowVisualComponent>();
            var component = UIThreadHelper.Instance.Invoke(() => new RHistoryWindowVisualComponent(historyTextBuffer, _historyProvider, _textEditorFactory, container));
            container.Component = component;
            return container;
        }
    }
}