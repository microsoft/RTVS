using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Collections;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Collections
{
    public class AsyncConcurrentQueueTest
    {
        [Test]
        public async Task DequeueAwaitEnqueue()
        {
            var queue = new AsyncConcurrentQueue<int>();
            Delay().ContinueWith(_ => queue.Enqueue(5)).DoNotWait();
            var result = await queue.DequeueAsync();
            result.Should().Be(5);
        }

        [Test]
        public async Task DequeueDequeueAwaitEnqueueEnqueue()
        {
            var queue = new AsyncConcurrentQueue<int>();
            var t1 = queue.DequeueAsync();
            var t2 = queue.DequeueAsync();

            t1.IsCompleted.Should().BeFalse();
            t2.IsCompleted.Should().BeFalse();

            Delay().ContinueWith(_ => queue.Enqueue(1)).DoNotWait();
            await Task.WhenAny(t1, t2);

            t1.IsCompleted.Should().BeTrue();
            t2.IsCompleted.Should().BeFalse();
            t1.Result.Should().Be(1);

            Delay().ContinueWith(_ => queue.Enqueue(2)).DoNotWait();

            await t2;
            t2.IsCompleted.Should().BeTrue();
            t1.Result.Should().Be(1);
        }

        [Test]
        public async Task EnqueueDequeueDequeueEnqueue()
        {
            var queue = new AsyncConcurrentQueue<int>();
            Task<int> t1 = null;
            Task<int> t2 = null;
            Action<Task> a1 = _ => queue.Enqueue(1);
            Action<Task> a2 = _ => t1 = queue.DequeueAsync();
            Action<Task> a3 = _ => t2 = queue.DequeueAsync();
            Action<Task> a4 = _ => queue.Enqueue(2);

            await Task.WhenAll(
                Delay().ContinueWith(a1), 
                Delay().ContinueWith(a2), 
                Delay().ContinueWith(a3), 
                Delay().ContinueWith(a4));

            t1.Should().NotBeNull();
            t1.IsCompleted.Should().BeTrue();
            t2.Should().NotBeNull();
            t2.IsCompleted.Should().BeTrue();

            new[] {t1.Result, t2.Result}.Should().BeEquivalentTo(1,2);
        }

        [Test]
        public async Task EnqueueDequeue1000()
        {
            var count = 1000;
            var queue = new AsyncConcurrentQueue<int>();
            var dequeueTasks = new ConcurrentQueue<Task<int>>();
            var input = Enumerable.Range(0, count).ToList();

            var tasks = input
                .SelectMany(i =>  new Action<Task>[] { _ => queue.Enqueue(i), _ => dequeueTasks.Enqueue(queue.DequeueAsync()) })
                .Select(a => Delay().ContinueWith(a));

            await Task.WhenAll(tasks);

            dequeueTasks.Should().HaveCount(count);
            dequeueTasks.Select(t => t.IsCompleted).Should().Equal(Enumerable.Repeat(true, count));
            dequeueTasks.Select(t => t.Result).Should().BeEquivalentTo(input);
        }

        private static Task Delay()
        {
            return Task.Delay(30);
        }
    }
}
