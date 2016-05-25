// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition.Hosting;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Components.Test.History {
    public class RHistoryViewModelTest : IDisposable {
        private readonly ExportProvider _exportProvider;
        private readonly IRHistory _history;
        private readonly IRHistoryWindowVisualComponent _historyVisualComponent;

        public RHistoryViewModelTest(RComponentsMefCatalogFixture catalog) {
            _exportProvider = catalog.CreateExportProvider();
            _history = _exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate().History;

            var containerFactory = _exportProvider.GetExportedValue<IRHistoryVisualComponentContainerFactory>();
            _historyVisualComponent = UIThreadHelper.Instance.Invoke(() => _history.GetOrCreateVisualComponent(containerFactory));
        }

        [CompositeTest(ThreadType.UI)]
        [InlineData(false, new[] {
@"f <- function() {
    print(42)
}",
@"y <- function() {
    print(7*9)
}"  },
new[] { 4 })]
        public void SelectNextHistoryEntry(bool isMultiline, string[] inputs, int[] linesToSelect) {
            foreach (var input in inputs) {
                _history.AddToHistory(input);
            }

            foreach (var line in linesToSelect) {
                _history.SelectHistoryEntry(line);
            }

            _history.SelectNextHistoryEntry();
        }

        public void Dispose() {
            _historyVisualComponent.Dispose();
            (_exportProvider as IDisposable)?.Dispose();
        }
    }
}