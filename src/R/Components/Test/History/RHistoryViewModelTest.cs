// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Microsoft.VisualStudio.Text.Formatting;
using Xunit;
using static Microsoft.UnitTests.Core.Threading.UIThreadTools;

namespace Microsoft.R.Components.Test.History {
    [ExcludeFromCodeCoverage]
    public class RHistoryViewModelTest : IAsyncLifetime {
        private readonly ContainerHostMethodFixture _containerHost;
        private readonly IRHistory _history;
        private readonly IRHistoryWindowVisualComponent _historyVisualComponent;
        private IDisposable _containerDisposable;

        public RHistoryViewModelTest(IExportProvider exportProvider, ContainerHostMethodFixture containerHost) {
            _containerHost = containerHost;
            _history = exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate().History;

            var containerFactory = exportProvider.GetExportedValue<IRHistoryVisualComponentContainerFactory>();
            _historyVisualComponent = UIThreadHelper.Instance.Invoke(() => _history.GetOrCreateVisualComponent(containerFactory));
        }
        
        public async Task InitializeAsync() {
            _historyVisualComponent.Control.Height = _historyVisualComponent.TextView.LineHeight * 5;
            _containerDisposable = await _containerHost.AddToHost(_historyVisualComponent.Control);
        }

        public Task DisposeAsync() {
            UIThreadHelper.Instance.Invoke(() => {
                _historyVisualComponent.Dispose();
            });

            _containerDisposable?.Dispose();
            return Task.CompletedTask;
        }

        [CompositeTest(ThreadType.UI)]
        [InlineData(new[] { 3 }, "    print(7*9)", 4)]
        [InlineData(new[] { 1, 3 }, "    print(7*9)", 4)]
        [InlineData(new[] { 1, 5 }, "h <- function() {", 6)]
        [InlineData(new[] { 6, 5 }, "    print(42)", 7)]
        [InlineData(new[] { 2, 6, 4 }, "    print(42)", 7)]
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
            var selectedSpans = _history.GetSelectedHistoryEntrySpans();
            var selectedSpan = selectedSpans.Should().ContainSingle().Which;
            selectedSpan.GetText().Should().Be(selectedText);

            var selectedTextViewLine = _historyVisualComponent.TextView.TextViewLines.GetTextViewLineContainingBufferPosition(selectedSpan.Start);
            selectedTextViewLine.VisibilityState.Should().Be(VisibilityState.FullyVisible);
        }

        [CompositeTest(ThreadType.UI)]
        [InlineData(new[] { 7 }, "h <- function() {", 6)]
        [InlineData(new[] { 7, 2 }, "    print(42)", 1)]
        [InlineData(new[] { 3, 4 }, "}", 2)]
        [InlineData(new[] { 4, 3 }, "}", 2)]
        [InlineData(new[] { 2, 6, 4 }, "    print(42)", 1)]
        public async Task SelectPreviousHistoryEntry(int[] linesToSelect, string selectedText, int selectedLineIndex) {
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
            _history.SelectPreviousHistoryEntry();

            await DoEvents();
            var selectedSpans = _history.GetSelectedHistoryEntrySpans();
            var selectedSpan = selectedSpans.Should().ContainSingle().Which;
            selectedSpan.GetText().Should().Be(selectedText);

            var selectedTextViewLine = _historyVisualComponent.TextView.TextViewLines.GetTextViewLineContainingBufferPosition(selectedSpan.Start);
            selectedTextViewLine.VisibilityState.Should().Be(VisibilityState.FullyVisible);
        }
    }
}