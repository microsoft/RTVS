// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Microsoft.VisualStudio.Text.Formatting;
using Xunit;
using static Microsoft.UnitTests.Core.Threading.UIThreadTools;

namespace Microsoft.R.Components.Test.History {
    public class RHistoryViewModelTest : IAsyncLifetime {
        private readonly ContainerHostMethodFixture _containerHost;
        private readonly ExportProvider _exportProvider;
        private readonly IRHistory _history;
        private readonly IRHistoryWindowVisualComponent _historyVisualComponent;
        private IDisposable _containerDisposable;

        public RHistoryViewModelTest(RComponentsMefCatalogFixture catalog, ContainerHostMethodFixture containerHost) {
            _containerHost = containerHost;
            _exportProvider = catalog.CreateExportProvider();
            _history = _exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate().History;

            var containerFactory = _exportProvider.GetExportedValue<IRHistoryVisualComponentContainerFactory>();
            _historyVisualComponent = UIThreadHelper.Instance.Invoke(() => _history.GetOrCreateVisualComponent(containerFactory));
        }
        
        public async Task InitializeAsync() {
            _historyVisualComponent.Control.Height = _historyVisualComponent.TextView.LineHeight * 5;
            _containerDisposable = await _containerHost.AddToHost(_historyVisualComponent.Control);
        }

        public Task DisposeAsync() {
            _containerDisposable?.Dispose();
            _historyVisualComponent.Dispose();
            (_exportProvider as IDisposable)?.Dispose();
            return Task.CompletedTask;
        }

        [CompositeTest(ThreadType.UI)]
        [InlineData(new[] { 3 }, "    print(7*9)", 4)]
        [InlineData(new[] { 1, 3 }, "    print(7*9)", 4)]
        [InlineData(new[] { 1, 5 }, "h <- function() {", 6)]
        public async Task SelectNextHistoryEntry(int[] linesToSelect, string selectedText, int selectedLineIndex) {
            var input = @"f <- function() {
    print(42)
}
g <- function() {
    print(7*9)
}
h <- function() {
    print(42)
}";
            _history.AddToHistory(input);

            foreach (var line in linesToSelect) {
                _history.SelectHistoryEntry(line);
            }

            await DoEvents();
            _history.SelectNextHistoryEntry();

            await DoEvents();

            var startPosition = _historyVisualComponent.TextView.TextBuffer.CurrentSnapshot.Lines.ElementAt(selectedLineIndex).Start;
            var selectedLine = _historyVisualComponent.TextView.TextViewLines.GetTextViewLineContainingBufferPosition(startPosition);
            selectedLine.VisibilityState.Should().Be(VisibilityState.FullyVisible);
            _history.GetSelectedText().Should().Be(selectedText);
        }
    }
}