using System;

namespace Microsoft.UnitTests.Core.XUnit {
    internal class NullTestMainThreadFixture : ITestMainThreadFixture {
        public static ITestMainThreadFixture Instance { get; } = new NullTestMainThreadFixture();

        private NullTestMainThreadFixture() { }

        public ITestMainThread CreateTestMainThread() => new NullTestMainThread();
        public bool CheckAccess() => false;
        public void Post(Action<object> action, object argument) => throw new NotSupportedException();

        private class NullTestMainThread : ITestMainThread {
            public void CancelPendingTasks() {}
            public void Dispose() {}
        }
    }
}