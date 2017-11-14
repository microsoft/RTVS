// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Common.Core.Disposables;

namespace Microsoft.UnitTests.Core.XUnit {
#if DESKTOP
    [System.Security.Permissions.HostProtection(Synchronization = true)]
#endif
    internal class TestTraceListener : TraceListener {
        private static IDisposable _instance;
        private static readonly object SyncObj = new object();

        private readonly TraceListener[] _wrappedListeners;

        public static void Ensure() {
            if (_instance == null) {
                lock (SyncObj) {
                    _instance = ReplaceListeners();
                }
            }
        }

        private static IDisposable ReplaceListeners() {
            var listeners = new TraceListener[Trace.Listeners.Count];
            Trace.Listeners.CopyTo(listeners, 0);
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TestTraceListener(listeners));

            var disposable = Disposable.Create(() => {
                lock (SyncObj) {
                    Trace.Listeners.Clear();
                    Trace.Listeners.AddRange(listeners);
                    _instance = null;
                }
            });

            void Restore(object o, EventArgs e) => disposable.Dispose();

            AppDomain.CurrentDomain.ProcessExit += Restore;
            AppDomain.CurrentDomain.DomainUnload += Restore;
            return disposable;
        }

        private TestTraceListener(TraceListener[] wrappedListeners) : base("XunitTest") {
            _wrappedListeners = wrappedListeners;
        }

        public override void Fail(string message) => throw new TraceFailException(message);

        public override void Fail(string message, string detailMessage) 
            => throw new TraceFailException(message, detailMessage);

        public override void Write(string message) {
            foreach (var listener in _wrappedListeners) {
                listener.Write(message);
            }
        }

        public override void WriteLine(string message) {
            foreach (var listener in _wrappedListeners) {
                listener.WriteLine(message);
            }
        }
    }
}
