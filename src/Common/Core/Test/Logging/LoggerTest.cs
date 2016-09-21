// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Logging;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Logging {
    [ExcludeFromCodeCoverage]
    public class LoggerTest {
        [Test]
        [Category.Logging]
        public void Logging_NullLogTest() {
            IActionLinesLog log = new NullLog();
            log.WriteAsync(MessageCategory.Error, "message").Wait();
            log.WriteFormatAsync(MessageCategory.Error, "message").Wait();
            log.WriteLineAsync(MessageCategory.Error, "message").Wait();

            log.Content.Should().BeEmpty();
            log.Lines.Should().BeEmpty();
        }

        [Test]
        [Category.Logging]
        public void Logging_LinesLogTest() {
            IActionLinesLog log = new LinesLog(NullLogWriter.Instance);

            log.WriteAsync(MessageCategory.Error, "message1").Wait();
            log.WriteLineAsync(MessageCategory.Error, " message2").Wait();
            log.WriteFormatAsync(MessageCategory.Error, "message3 {0}\r\n", 1).Wait();
            log.WriteLineAsync(MessageCategory.Error, "message4").Wait();

            log.Content.Should().Be("message1 message2\r\nmessage3 1\r\nmessage4\r\n");
            log.Lines.Should().Equal("message1 message2", "message3 1", "message4", string.Empty);
        }
    }
}
