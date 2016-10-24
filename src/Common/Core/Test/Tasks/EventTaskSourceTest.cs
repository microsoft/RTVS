// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Tasks;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Tasks {
    [ExcludeFromCodeCoverage]
    public class EventTaskSourceTest {
        private readonly EventTaskSource<ObjectWithEvent> _eas;

        public EventTaskSourceTest() {
            _eas = new EventTaskSource<ObjectWithEvent>((o, h) => o.Event += h, (o, h) => o.Event -= h);
        }

        [Test]
        public async Task TwoObjects() {
            var obj1 = new ObjectWithEvent();
            var obj2 = new ObjectWithEvent();
            var t1 = _eas.Create(obj1);
            var t2 = _eas.Create(obj2);

            t1.IsCompleted.Should().BeFalse();
            t2.IsCompleted.Should().BeFalse();

            Task.Run(() => obj1.Raise(5)).DoNotWait();

            await Task.WhenAny(t1, t2);
            t1.IsCompleted.Should().BeTrue();
            t2.IsCompleted.Should().BeFalse();
            t1.Result.Should().BeOfType<IntEventArgs>()
                .Which.Value.Should().Be(5);
            
            Task.Run(() => obj2.Raise(10)).DoNotWait();

            await t2;
            t1.IsCompleted.Should().BeTrue();
            t2.IsCompleted.Should().BeTrue();

            t2.Result.Should().BeOfType<IntEventArgs>()
                .Which.Value.Should().Be(10);
        }

        [Test]
        public async Task TwoObjects_OneCancelled() {
            var obj1 = new ObjectWithEvent();
            var t1 = _eas.Create(obj1);

            var obj2 = new ObjectWithEvent();
            var cts = new CancellationTokenSource();
            var t2 = _eas.Create(obj2, cts.Token);

            t1.IsCompleted.Should().BeFalse();
            t2.IsCompleted.Should().BeFalse();

            Task.Run(() => obj1.Raise(5)).DoNotWait();

            await Task.WhenAny(t1, t2);
            t1.IsCompleted.Should().BeTrue();
            t2.IsCompleted.Should().BeFalse();
            t1.Result.Should().BeOfType<IntEventArgs>()
                .Which.Value.Should().Be(5);

            Task.Run(() => cts.Cancel()).DoNotWait();
            Func<Task> f = async () => await t2;
            f.ShouldThrow<OperationCanceledException>();

            t1.IsCompleted.Should().BeTrue();
            t2.IsCompleted.Should().BeTrue();
        }

        [Test]
        public async Task TwoTasks() {
            var obj = new ObjectWithEvent();
            var t1 = _eas.Create(obj);
            var t2 = _eas.Create(obj);

            t1.IsCompleted.Should().BeFalse();
            t2.IsCompleted.Should().BeFalse();
            Task.Run(() => obj.Raise(5)).DoNotWait();

            await Task.WhenAll(t1, t2);
            t1.Result.Should().BeOfType<IntEventArgs>()
                .Which.Value.Should().Be(5);
            t2.Result.Should().Be(t1.Result);
        }

        [Test]
        public async Task TwoEvents() {
            var obj = new ObjectWithEvent();
            var t = _eas.Create(obj);

            t.IsCompleted.Should().BeFalse();
            Task.Run(() => obj.Raise(5)).DoNotWait();

            await t;
            t.Result.Should().BeOfType<IntEventArgs>()
                .Which.Value.Should().Be(5);

            Task.Run(() => obj.Raise(10)).DoNotWait();
            t.Result.Should().BeOfType<IntEventArgs>()
                .Which.Value.Should().Be(5);
        }

        [Test]
        public async Task TwoEventsSequentalTasks() {
            var obj = new ObjectWithEvent();
            var t1 = _eas.Create(obj);

            t1.IsCompleted.Should().BeFalse();
            Task.Run(() => obj.Raise(5)).DoNotWait();

            await t1;
            t1.Result.Should().BeOfType<IntEventArgs>()
                .Which.Value.Should().Be(5);

            var t2 = _eas.Create(obj);
            Task.Run(() => obj.Raise(10)).DoNotWait();
            t1.Result.Should().BeOfType<IntEventArgs>()
                .Which.Value.Should().Be(5);
            t2.Result.Should().BeOfType<IntEventArgs>()
                .Which.Value.Should().Be(10);
        }

        [Test]
        public async Task TwoEvents_NoUnsubscribe() {
            var obj = new ObjectWithEvent();
            var t = _eas.Create(obj);

            t.IsCompleted.Should().BeFalse();
            Task.Run(() => obj.Raise(5)).DoNotWait();

            await t;
            t.Result.Should().BeOfType<IntEventArgs>()
                .Which.Value.Should().Be(5);

            Task.Run(() => obj.Raise(10)).DoNotWait();
            t.Result.Should().BeOfType<IntEventArgs>()
                .Which.Value.Should().Be(5);
        }

        public class Typed {
            private readonly EventTaskSource<ObjectWithEvent, IntEventArgs> _eas;

            public Typed() {
                _eas = new EventTaskSource<ObjectWithEvent, IntEventArgs>((o, h) => o.IntEvent += h, (o, h) => o.IntEvent -= h);
            }

            [Test]
            public async Task TwoObjects() {
                var obj1 = new ObjectWithEvent();
                var obj2 = new ObjectWithEvent();
                var t1 = _eas.Create(obj1);
                var t2 = _eas.Create(obj2);

                t1.IsCompleted.Should().BeFalse();
                t2.IsCompleted.Should().BeFalse();

                Task.Run(() => obj1.RaiseInt(5)).DoNotWait();

                await Task.WhenAny(t1, t2);
                t1.IsCompleted.Should().BeTrue();
                t2.IsCompleted.Should().BeFalse();
                t1.Result.Value.Should().Be(5);

                Task.Run(() => obj2.RaiseInt(10)).DoNotWait();

                await t2;
                t1.IsCompleted.Should().BeTrue();
                t2.IsCompleted.Should().BeTrue();

                t2.Result.Value.Should().Be(10);
            }

            [Test]
            public async Task TwoObjects_OneCancelled() {
                var obj1 = new ObjectWithEvent();
                var t1 = _eas.Create(obj1);

                var obj2 = new ObjectWithEvent();
                var cts = new CancellationTokenSource();
                var t2 = _eas.Create(obj2, cts.Token);

                t1.IsCompleted.Should().BeFalse();
                t2.IsCompleted.Should().BeFalse();

                Task.Run(() => obj1.RaiseInt(5)).DoNotWait();

                await Task.WhenAny(t1, t2);
                t1.IsCompleted.Should().BeTrue();
                t2.IsCompleted.Should().BeFalse();
                t1.Result.Value.Should().Be(5);

                Task.Run(() => cts.Cancel()).DoNotWait();
                Func<Task> f = async () => await t2;
                f.ShouldThrow<OperationCanceledException>();

                t1.IsCompleted.Should().BeTrue();
                t2.IsCompleted.Should().BeTrue();
            }

            [Test]
            public async Task TwoTasks() {
                var obj = new ObjectWithEvent();
                var t1 = _eas.Create(obj);
                var t2 = _eas.Create(obj);

                t1.IsCompleted.Should().BeFalse();
                t2.IsCompleted.Should().BeFalse();
                Task.Run(() => obj.RaiseInt(5)).DoNotWait();

                await Task.WhenAll(t1, t2);
                t1.Result.Value.Should().Be(5);
                t2.Result.Should().Be(t1.Result);
            }

            [Test]
            public async Task TwoEvents() {
                var obj = new ObjectWithEvent();
                var t = _eas.Create(obj);

                t.IsCompleted.Should().BeFalse();
                Task.Run(() => obj.RaiseInt(5)).DoNotWait();

                await t;
                t.Result.Value.Should().Be(5);

                Task.Run(() => obj.RaiseInt(10)).DoNotWait();
                t.Result.Value.Should().Be(5);
            }

            [Test]
            public async Task TwoEventsSequentalTasks() {
                var obj = new ObjectWithEvent();
                var t1 = _eas.Create(obj);

                t1.IsCompleted.Should().BeFalse();
                Task.Run(() => obj.RaiseInt(5)).DoNotWait();

                await t1;
                t1.Result.Value.Should().Be(5);

                var t2 = _eas.Create(obj);
                Task.Run(() => obj.RaiseInt(10)).DoNotWait();
                t1.Result.Value.Should().Be(5);
                t2.Result.Value.Should().Be(10);
            }

            [Test]
            public async Task TwoEvents_NoUnsubscribe() {
                var obj = new ObjectWithEvent();
                var t = _eas.Create(obj);

                t.IsCompleted.Should().BeFalse();
                Task.Run(() => obj.RaiseInt(5)).DoNotWait();

                await t;
                t.Result.Value.Should().Be(5);

                Task.Run(() => obj.RaiseInt(10)).DoNotWait();
                t.Result.Value.Should().Be(5);
            }
        }

        private class ObjectWithEvent {
            public event EventHandler Event;
            public event EventHandler<IntEventArgs> IntEvent;

            public void Raise(int value = 0) {
                Event?.Invoke(this, new IntEventArgs(value));
            }

            public void RaiseInt(int value) {
                IntEvent?.Invoke(this, new IntEventArgs(value));
            }
        }

        private class IntEventArgs : EventArgs {
            public int Value { get; }

            public IntEventArgs(int value) {
                Value = value;
            }
        }
    }
}
