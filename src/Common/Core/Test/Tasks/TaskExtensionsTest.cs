// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Common.Core.Test.Tasks {
    [Collection(CollectionNames.NonParallel)]
    public class TaskExtensionsTest {
        [Test]
        public async Task DoNotWait() {
            var expected = new InvalidOperationException();

            Func<Task> backgroundThreadAction = async () => {
                await TaskUtilities.SwitchToBackgroundThread();
                throw expected;
            };

            Func<Task> uiThreadAction = async () => {
                try {
                    Thread.CurrentThread.Should().Be(UIThreadHelper.Instance.Thread);
                    await backgroundThreadAction();
                } finally {
                    Thread.CurrentThread.Should().Be(UIThreadHelper.Instance.Thread);
                }
            };

            var exceptionTask = UIThreadHelper.Instance.WaitForNextExceptionAsync();

            UIThreadHelper.Instance.Invoke(() => uiThreadAction().DoNotWait());
            var actual = await exceptionTask;
            actual.Should().Be(expected);
        }
    }
}
