// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Logging;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;
using Xunit;

namespace Microsoft.Common.Core.Test.Logging {
    [ExcludeFromCodeCoverage]
    [Category.Logging]
    public class LoggerTest {
        [CompositeTest]
        [InlineData(LogVerbosity.None)]
        [InlineData(LogVerbosity.Minimal)]
        [InlineData(LogVerbosity.Normal)]
        [InlineData(LogVerbosity.Traffic)]
        public void Verbosity(LogVerbosity verbosity) {
            var writer = Substitute.For<IActionLogWriter>();
            var perm = Substitute.For<ILoggingPermissions>();
            perm.CurrentVerbosity.Returns(verbosity);

            var log = new Logger(writer, perm);
            log.Write(LogVerbosity.None, MessageCategory.Error, "message0");
            log.Write(LogVerbosity.Minimal, MessageCategory.Error, "message1");
            log.Write(LogVerbosity.Normal, MessageCategory.Error, "message2");
            log.Write(LogVerbosity.Traffic, MessageCategory.Error, "message3");

            int i = 0;
            foreach (var v in Enum.GetValues(typeof(LogVerbosity))) {
                if ((int)v > (int)LogVerbosity.None && (int)v <= (int)verbosity) {
                    writer.Received().Write(MessageCategory.Error, "message" + i);
                } else {
                    writer.DidNotReceive().Write(MessageCategory.Error, "message" + i);
                }
                i++;
            }

            writer.DidNotReceive().Flush();

            if (verbosity > LogVerbosity.None) {
                log.Flush();
                writer.Received().Flush();
            }
        }
    }
}
