using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Threading;

namespace Microsoft.Common.Core.Test.STA {
    //internal sealed class EditorTest {
    //    [AssemblyCleanup]
    //    public static void Cleanup() {
    //        StaThread.Terminate();
    //    }
    //}

    [ExcludeFromCodeCoverage]
    public static class StaThread {
        [ExcludeFromCodeCoverage]
        class Request {
            public readonly Action<object, ManualResetEventSlim> Action;
            public readonly object Parameter;
            public readonly ManualResetEventSlim Event = new ManualResetEventSlim(false);

            public Request(Action<object, ManualResetEventSlim> action, object parameter) {
                Action = action;
                Parameter = parameter;
            }
        }

        public static readonly ManualResetEventSlim ThreadAvailable = new ManualResetEventSlim(false);

        private static readonly object _creatorLock = new object();
        private static readonly ConcurrentQueue<Request> _requestQueue = new ConcurrentQueue<Request>();
        private static readonly ManualResetEventSlim ThreadExit = new ManualResetEventSlim(false);

        private static Thread _thread;

        /// <summary>
        /// Enqueues test execution for a sequential execution in an STA thread.
        /// </summary>
        public static void RunStaTest(Action<object, ManualResetEventSlim> action, object parameter = null) {
            Request request = new Request(action, parameter);
            _requestQueue.Enqueue(request);

            CreateStaThread();
            request.Event.Wait();
        }

        private static void CreateStaThread() {
            if (!ThreadExit.IsSet) {
                lock (_creatorLock) {
                    if (_thread == null) {
                        _thread = new Thread(ThreadEntry);
                        _thread.SetApartmentState(ApartmentState.STA);
                        _thread.IsBackground = false;
                        _thread.DisableComObjectEagerCleanup();

                        _thread.Start();
                        ThreadAvailable.Set();
                    }
                }
            }
        }

        public static void Terminate() {
            if (!ThreadExit.IsSet) {
                ThreadExit.Set();
                _thread.Join();
            }
        }

        /// <summary>
        /// Invokes an action in the STA thread
        /// </summary>
        /// <param name="action"></param>
        public static void Invoke(Action<object> action, object parameter) {
            var dispatcher = Dispatcher.FromThread(_thread);
            if (dispatcher != null) {
                dispatcher.Invoke(action, parameter);
            }
        }

        /// <summary>
        /// Invokes an action in the STA thread
        /// </summary>
        /// <param name="action"></param>
        public static void Invoke(Action action) {
            var dispatcher = Dispatcher.FromThread(_thread);
            if (dispatcher != null) {
                dispatcher.Invoke(action, null);
            }
        }

        private static void ThreadEntry() {
            var timeStart = DateTime.Now;

            while (!ThreadExit.IsSet) {
                ThreadAvailable.Wait();

                if (_requestQueue.Count > 0) {
                    Request request;
                    _requestQueue.TryDequeue(out request);

                    ThreadAvailable.Reset();

                    request.Action(request.Parameter, request.Event);
                } else {
                    ThreadExit.Wait(200);
                }
            }

            try {
                var dispatcher = Dispatcher.FromThread(_thread);
                if (dispatcher != null) {
                    if (!dispatcher.HasShutdownStarted) {
                        dispatcher.InvokeShutdown();
                    }
                    while (!dispatcher.HasShutdownFinished) {
                        Thread.Sleep(10);
                    }
                }
            } catch (Exception) { }
        }
    }
}
