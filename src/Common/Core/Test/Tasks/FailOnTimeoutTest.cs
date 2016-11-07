// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Tasks {
    [ExcludeFromCodeCoverage]
    public class FailOnTimeoutTest {
        [Test]
        public void FailOnTimeout() {
            Func<Task> f = () => Task.Delay(200).FailOnTimeout(100);
            f.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void FailOnCustomException() {
            Func<Task> createTask = async () => {
                await Task.Delay(100);
                throw new InvalidOperationException();
            };

            Func<Task> f = () => createTask().FailOnTimeout(400);
            f.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void FailOnCancellation() {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(100);

            Func<Task> f = () => Task.Delay(700, cts.Token).FailOnTimeout(400);
            f.ShouldThrow<TaskCanceledException>();
        }

        [Test]
        public void Success() {
            Func<Task> f = () => Task.Delay(100).FailOnTimeout(200);
            f.ShouldNotThrow();
        }
    }
}