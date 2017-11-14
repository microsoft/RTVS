using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.UnitTests.Core.XUnit {
    internal class NullTestMainThreadFixture : ITestMainThreadFixture {
        public static ITestMainThreadFixture Instance { get; } = new NullTestMainThreadFixture();

        private NullTestMainThreadFixture() { }

        public ITestMainThread CreateTestMainThread() => new NullTestMainThread();
        public bool CheckAccess() => false;
        public Task<T> Invoke<T>(Func<Task<T>> action) => throw new NotSupportedException();
        public void Post(SendOrPostCallback action, object argument) => throw new NotSupportedException();

        private class NullTestMainThread : ITestMainThread {
            public void CancelPendingTasks() {}
            public void Dispose() {}
        }
    }
}