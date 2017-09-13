// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.LanguageServer.Threading;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.LanguageServer.Test.Text {
    [Category.VsCode.Threading]
    public sealed class MainThreadTest: IDisposable {
        private readonly MainThread _mt = new MainThread();

        public void Dispose() {
            _mt.Dispose();
        }

        [Test]
        public void Send() {
            _mt.SynchronizationContext.Should().BeOfType(typeof(MainThread.MainThreadSynchronizationContext));
            _mt.ThreadId.Should().Be(_mt.Thread.ManagedThreadId);

            var tcs = new TaskCompletionSource<bool>();
            _mt.Send(o => tcs.TrySetResult(true), null);
            tcs.Task.IsCompleted.Should().BeTrue();

            tcs = new TaskCompletionSource<bool>();
            _mt.SynchronizationContext.Send(o => tcs.TrySetResult(true), null);
            tcs.Task.IsCompleted.Should().BeTrue();
        }

        [Test]
        public void Post() {
            var tcs = new TaskCompletionSource<bool>();
            _mt.Post(o => {
                Task.Delay(1000);
                tcs.TrySetResult(true);
            }, null);

            tcs.Task.IsCompleted.Should().BeFalse();
            tcs.Task.Wait(2000);
            tcs.Task.IsCompleted.Should().BeTrue();
        }

        [Test]
        public void Terminate() {
            _mt.Thread.IsAlive.Should().BeTrue();
            _mt.Dispose();
            _mt.Thread.IsAlive.Should().BeFalse();

            Action a1 = () => _mt.Post(() => { }, CancellationToken.None);
            a1.ShouldThrow<ObjectDisposedException>();

            Action a2 = () => _mt.Post(o => { }, null);
            a2.ShouldThrow<ObjectDisposedException>();

            Action a3 = () => _mt.Send(o => { }, null);
            a3.ShouldThrow<ObjectDisposedException>();
        }
    }
}
