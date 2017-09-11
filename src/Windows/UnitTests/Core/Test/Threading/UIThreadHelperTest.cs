// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.UnitTests.Core.Test.Threading {
    [ExcludeFromCodeCoverage]
    [Category.Threads]
    [Collection(CollectionNames.NonParallel)]
    public class UIThreadHelperTest {
        [Test]
        public async Task InvokeAsync() {
            var task = UIThreadHelper.Instance.InvokeAsync(() => Thread.Sleep(500));
            await task;
            task.Should().BeRanToCompletion();
        }

        [Test]
        public async Task DoEvents_BackgroundThread() {
            var t1 = UIThreadHelper.Instance.InvokeAsync(() => UIThreadHelper.Instance.DoEvents(200));
            var t2 = UIThreadHelper.Instance.InvokeAsync(() => {});
            
            await t2;
            t1.IsCompleted.Should().BeFalse();
            t2.Status.Should().Be(TaskStatus.RanToCompletion);

            await t1;
            t1.Status.Should().Be(TaskStatus.RanToCompletion);
        }

        [Test]
        public async Task DoEvents_UIThread() {
            var t1 = Task.Run(() => UIThreadHelper.Instance.DoEvents(200));
            var t2 = Task.Run(() => {});
            
            await t2;
            t1.IsCompleted.Should().BeFalse();
            t2.Status.Should().Be(TaskStatus.RanToCompletion);

            await t1;
            t1.Status.Should().Be(TaskStatus.RanToCompletion);
        }
    }
}
