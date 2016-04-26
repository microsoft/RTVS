// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.Common.Wpf.Threading;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Wpf.Test.Threading {
    public class MainThreadAwaitableTest {
        private readonly Thread _dispatcherThread;
        private readonly Dispatcher _dispatcher;

        public MainThreadAwaitableTest() {
            _dispatcherThread = UIThreadHelper.Instance.Thread;
            _dispatcher = Dispatcher.FromThread(_dispatcherThread);
        }

        [Test]
        public void IsCompleleted_BackgroundThread() {
            var awaitable = new MainThreadAwaitable(_dispatcher);

            Thread.CurrentThread.Should().NotBe(_dispatcherThread);
            awaitable.GetAwaiter().IsCompleted.Should().Be(false);
        }

        [Test(ThreadType.UI)]
        public void IsCompleleted_DispatcherThread() {
            var awaitable = new MainThreadAwaitable(_dispatcher);

            Thread.CurrentThread.Should().Be(_dispatcherThread);
            awaitable.GetAwaiter().IsCompleted.Should().Be(true);
        }

        [Test]
        public async Task Await_BackgroundThread() {
            var awaitable = new MainThreadAwaitable(_dispatcher);

            await awaitable;

            Thread.CurrentThread.Should().Be(_dispatcherThread);
            Action a = () => awaitable.GetAwaiter().GetResult();
            a.ShouldNotThrow();
        }

        [Test(ThreadType.UI)]
        public async Task Await_OnDispatcherThread() {
            var awaitable = new MainThreadAwaitable(_dispatcher);

            await awaitable;

            Thread.CurrentThread.Should().Be(_dispatcherThread);
            Action a = () => awaitable.GetAwaiter().GetResult();
            a.ShouldNotThrow();
        }

        [Test]
        public void GetResult_ThrowOnBackgroundThread() {
            var thread = Dispatcher.FromThread(UIThreadHelper.Instance.Thread);
            var awaitable = new MainThreadAwaitable(thread);

            Action a = () => awaitable.GetAwaiter().GetResult();
            a.ShouldThrow<InvalidOperationException>();
        }
    }
}
