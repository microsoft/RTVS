// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Common.Core.Logging;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;
using Xunit;

namespace Microsoft.Common.Core.Test.Logging {
    [ExcludeFromCodeCoverage]
    [Category.Logging]
    public class LoggerTest {
        [CompositeTest]
        [InlineData(LogLevel.None)]
        [InlineData(LogLevel.Minimal)]
        [InlineData(LogLevel.Normal)]
        [InlineData(LogLevel.Traffic)]
        public async Task LogLevels(LogLevel level) {
            var writer = Substitute.For<IActionLogWriter>();

            var log = new Logger(string.Empty, level, writer);
            await log.WriteAsync(LogLevel.Minimal, MessageCategory.Error, "message1");
            await log.WriteFormatAsync(LogLevel.Normal, MessageCategory.Error, "message2");
            await log.WriteLineAsync(LogLevel.Traffic, MessageCategory.Error, "message3");

            for(int i = 0; i < (int)level; i++) {
                await writer.DidNotReceive().WriteAsync(Arg.Any<MessageCategory>(), Arg.Any<string>());
            }
            for (int i = (int)level; i < (int)Enum.GetValues(typeof(LogLevel)).Length; i++) {
                await writer.Received().WriteAsync(MessageCategory.Error, "message" + i.ToString());
            }

            writer.DidNotReceive().Flush();
            log.Flush();
            writer.Received().Flush();
        }
    }
}
