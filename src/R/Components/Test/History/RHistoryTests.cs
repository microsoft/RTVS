// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Components.History.Implementation;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Test.StubFactories;
using Microsoft.R.Components.Test.Stubs;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text;
using Xunit;

namespace Microsoft.R.Components.Test.History {
    [ExcludeFromCodeCoverage]
    public class RHistoryTests {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly ITextBuffer _textBuffer;

        public RHistoryTests(IExportProvider exportProvider) {
            _interactiveWorkflow = InteractiveWorkflowStubFactory.CreateDefault();
            _textBuffer = exportProvider.GetExportedValue<ITextBufferFactoryService>().CreateTextBuffer();
        }

        [CompositeTest]
        [Category.History]
        [InlineData(new [] { " ", "\r\n", " \r\n " }, "")]
        [InlineData(new [] { "x <- 1" }, "x <- 1")]
        [InlineData(new [] { "x <- 1\r\ny <- 2" }, "x <- 1\r\ny <- 2")]
        [InlineData(new [] { "x <- 1\r\n \r\ny <- 2" }, "x <- 1\r\ny <- 2")]
        [InlineData(new [] {
@"f <- function() {
    print(42)
}" },
@"f <- function() {
    print(42)
}")]
[InlineData(new [] {
@"  f <- function() {

    print(42)

  }" },
@"  f <- function() {
    print(42)
  }")]
        [InlineData(new [] { "x <- 1", "y <- 2" }, "x <- 1\r\ny <- 2")]
        [InlineData(new [] { "  x <- 1", "   y <- 2" }, "  x <- 1\r\n   y <- 2")]
        public void AddToHistory(string[] inputs, string expected) {
            var history = new RHistory(_interactiveWorkflow, _textBuffer, null, new RSettingsStub(), null, null, () => { });

            foreach (var input in inputs) {
                history.AddToHistory(input);
            }

            _textBuffer.CurrentSnapshot.GetText().Should().Be(expected);
        }

        [CompositeTest]
        [Category.History]
        [InlineData(true, new[] { "x <- 1" }, 0, "x <- 1")]
        [InlineData(false, new[] { "x <- 1" }, 0, "x <- 1")]
        [InlineData(true, new[] { "x <- 1", "y <- 2" }, 0, "x <- 1")]
        [InlineData(false, new[] { "x <- 1", "y <- 2" }, 0, "x <- 1")]
        [InlineData(true, new[] { "x <- 1", "y <- 2" }, 1, "y <- 2")]
        [InlineData(false, new[] { "x <- 1", "y <- 2" }, 1, "y <- 2")]
        [InlineData(true, new[] { "x <- 1\r\ny <- 2" }, 0, "x <- 1\r\ny <- 2")]
        [InlineData(false, new[] { "x <- 1\r\n \r\ny <- 2" }, 0, "x <- 1")]
        [InlineData(true, new[] { "x <- 1\r\ny <- 2" }, 1, "x <- 1\r\ny <- 2")]
        [InlineData(false, new[] { "x <- 1\r\n \r\ny <- 2" }, 1, "y <- 2")]
        [InlineData(true, new[] {
@"f <- function() {
    print(42)
}",
@"y <- function() {
    print(7*9)
}"  },
2,
@"f <- function() {
    print(42)
}")]
       [InlineData(false, new[] {
@"f <- function() {
    print(42)
}",
@"y <- function() {
    print(7*9)
}"  },
2,
@"}")]
       [InlineData(true, new[] {
@"f <- function() {
    print(42)
}",
@"y <- function() {
    print(7*9)
}"  },
4,
@"y <- function() {
    print(7*9)
}")]
       [InlineData(false, new[] {
@"f <- function() {
    print(42)
}",
@"y <- function() {
    print(7*9)
}"  },
4,
@"    print(7*9)")]
        public void SelectHistoryEntry(bool isMultiline, string[] inputs, int lineToSelect, string expected) {
            var settings = new RSettingsStub { MultilineHistorySelection = isMultiline };
            var history = new RHistory(_interactiveWorkflow, _textBuffer, null, settings, null, null, () => { });

            foreach (var input in inputs) {
                history.AddToHistory(input);
            }

            history.SelectHistoryEntry(lineToSelect);

            history.GetSelectedText().Should().Be(expected);
            history.GetSelectedHistoryEntrySpans().Should().ContainSingle()
                .Which.GetText().Should().Be(expected);
        }
    }
}
