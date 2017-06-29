// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
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
        private readonly IRHistoryVisual _history;
        private readonly IRHistoryWindowVisualComponent _historyVisualComponent;
        private IDisposable _containerDisposable;

        public RHistoryViewModelTest(IServiceContainer services, ContainerHostMethodFixture containerHost) {
            _containerHost = containerHost;
            _history = (IRHistoryVisual)services.GetService<IRInteractiveWorkflowVisualProvider>().GetOrCreate().History;

            var containerFactory = services.GetService<IRHistoryVisualComponentContainerFactory>();
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
        [InlineData(new[] { 6, 5 }, "h <- function() {", 6)]
        [InlineData(new[] { 5, 6 }, "    print(42)", 7)]
        [InlineData(new[] { 2, 6, 4 }, "}", 5)]
        [InlineData(new[] { 2, 4, 6 }, "    print(42)", 7)]
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
        [InlineData(new[] { 3, 4 }, "g <- function() {", 3)]
        [InlineData(new[] { 4, 3 }, "}", 2)]
        [InlineData(new[] { 2, 6, 4 }, "g <- function() {", 3)]
        [InlineData(new[] { 6, 4, 2 }, "    print(42)", 1)]
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
        
        [CompositeTest(ThreadType.UI)]
        [InlineData(new[] { 8 }, @"}")]
        [InlineData(new[] { 3 }, @"g <- function() {
    print(7*9)")]
        [InlineData(new[] { 1, 2 }, @"}
g <- function() {")]
        [InlineData(new[] { 1, 3 }, @"g <- function() {
    print(7*9)")]
        [InlineData(new[] { 7, 8 }, @"    print(42)
}")]
        [InlineData(new[] { 8, 7 }, @"    print(42)
}")]
        [InlineData(new[] { 2, 4, 6 }, @"h <- function() {
    print(42)")]
        [InlineData(new[] { 2, 6, 4 }, @"    print(7*9)
}")]
        [InlineData(new[] { 6, 4, 2, 4 }, @"}
g <- function() {")]
        [InlineData(new[] { 6, 4, 2, 2 }, @"    print(7*9)
}")]
        [InlineData(new[] { 6, 2, 4, 4 }, @"}
g <- function() {")]
        [InlineData(new[] { 6, 2, 4, 2 }, @"    print(7*9)
}")]
        public async Task ToggleHistoryEntriesRangeSelectionDown(int[] linesToSelect, string selectedText) {
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
                _history.ToggleHistoryEntrySelection(line);
            }

            await DoEvents();
            _history.ToggleHistoryEntriesRangeSelectionDown();

            await DoEvents();
            _history.GetSelectedHistoryEntrySpans().Should().ContainSingle()
                .Which.GetText().Should().Be(selectedText);
        }

        [CompositeTest(ThreadType.UI)]
        [InlineData(new[] { 0 }, @"f <- function() {")]
        [InlineData(new[] { 3 }, @"}
g <- function() {")]
        [InlineData(new[] { 3, 4 }, @"g <- function() {
    print(7*9)")]
        [InlineData(new[] { 3, 5 }, @"    print(7*9)
}")]
        [InlineData(new[] { 1, 0 }, @"f <- function() {
    print(42)")]
        [InlineData(new[] { 0, 1 }, @"f <- function() {
    print(42)")]
        [InlineData(new[] { 2, 4, 6 }, @"}
h <- function() {")]
        [InlineData(new[] { 2, 6, 4 }, @"g <- function() {
    print(7*9)")]
        [InlineData(new[] { 2, 4, 6, 4 }, @"}
h <- function() {")]
        [InlineData(new[] { 2, 4, 6, 6 }, @"g <- function() {
    print(7*9)")]
        [InlineData(new[] { 2, 6, 4, 4 }, @"}
h <- function() {")]
        [InlineData(new[] { 2, 6, 4, 6 }, @"g <- function() {
    print(7*9)")]
        public async Task ToggleHistoryEntriesRangeSelectionUp(int[] linesToSelect, string selectedText) {
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
                _history.ToggleHistoryEntrySelection(line);
            }

            await DoEvents();
            _history.ToggleHistoryEntriesRangeSelectionUp();

            await DoEvents();
            _history.GetSelectedHistoryEntrySpans().Should().ContainSingle()
                .Which.GetText().Should().Be(selectedText);
        }        

        [CompositeTest(ThreadType.UI)]
        [InlineData(3, 3, @"g <- function() {
    print(7*9)")]
        [InlineData(8, 8, @"}")]
        [InlineData(1, 2, @"    print(42)
}
g <- function() {")]
        [InlineData(2, 1, @"}")]
        [InlineData(1, 3, @"    print(42)
}
g <- function() {
    print(7*9)")]
        [InlineData(3, 1, @"}
g <- function() {")]
        [InlineData(7, 8, @"    print(42)
}")]
        [InlineData(8, 7, @"}")]
        [InlineData(2, 6, @"}
g <- function() {
    print(7*9)
}
h <- function() {
    print(42)")]
        [InlineData(6, 2, @"g <- function() {
    print(7*9)
}
h <- function() {")]
        public async Task ToggleHistoryEntriesRangeSelectionDown_AfterRangeSelection(int rangeStart, int rangeEnd, string selectedText) {
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
            _history.SelectHistoryEntry(rangeStart);
            _history.SelectHistoryEntriesRangeTo(rangeEnd);

            await DoEvents();
            _history.ToggleHistoryEntriesRangeSelectionDown();

            await DoEvents();
            _history.GetSelectedHistoryEntrySpans().Should().ContainSingle()
                .Which.GetText().Should().Be(selectedText);
        }

        [CompositeTest(ThreadType.UI)]
        [InlineData(3, 3, @"}
g <- function() {")]
        [InlineData(0, 0, @"f <- function() {")]
        [InlineData(1, 2, @"    print(42)")]
        [InlineData(2, 1, @"f <- function() {
    print(42)
}")]
        [InlineData(1, 3, @"    print(42)
}")]
        [InlineData(3, 1, @"f <- function() {
    print(42)
}
g <- function() {")]
        [InlineData(0, 1, @"f <- function() {")]
        [InlineData(1, 0, @"f <- function() {
    print(42)")]
        [InlineData(2, 6, @"}
g <- function() {
    print(7*9)
}")]
        [InlineData(6, 2, @"    print(42)
}
g <- function() {
    print(7*9)
}
h <- function() {")]
        public async Task ToggleHistoryEntriesRangeSelectionUp_AfterRangeSelection(int rangeStart, int rangeEnd, string selectedText) {
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
            _history.SelectHistoryEntry(rangeStart);
            _history.SelectHistoryEntriesRangeTo(rangeEnd);

            await DoEvents();
            _history.ToggleHistoryEntriesRangeSelectionUp();

            await DoEvents();
            _history.GetSelectedHistoryEntrySpans().Should().ContainSingle()
                .Which.GetText().Should().Be(selectedText);
        }
    }
}