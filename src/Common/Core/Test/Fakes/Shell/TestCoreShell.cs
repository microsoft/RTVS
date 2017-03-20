// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Core.UI;
using Microsoft.UnitTests.Core.Threading;
using NSubstitute;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    [ExcludeFromCodeCoverage]
    public sealed class TestCoreShell : ICoreShell, IIdleTimeSource {
        private readonly CompositionContainer _container;
        private readonly TestServiceManager _serviceManager;
        private readonly Thread _creatorThread;

        public TestCoreShell(CompositionContainer container
            , IActionLog log = null
            , ILoggingPermissions loggingPermissions = null
            , IFileSystem fs = null
            , IRegistry registry = null
            , IProcessServices ps = null) {
            _container = container;
            _creatorThread = UIThreadHelper.Instance.Thread;
            _serviceManager = new TestServiceManager(container);
            _serviceManager
                .AddService(log ?? Substitute.For<IActionLog>())
                .AddService(loggingPermissions ?? Substitute.For<ILoggingPermissions>())
                .AddService(fs ?? new FileSystem())
                .AddService(registry ?? new RegistryImpl())
                .AddService(ps ?? new ProcessServices())
                .AddService(new TestUIServices())
                .AddService(new TestPlatformServices());
        }

        public string ApplicationName => "RTVS_Test";
        public int LocaleId => 1033;

        public IServiceContainer Services => _serviceManager;

        public void DispatchOnUIThread(Action action) => UIThreadHelper.Instance.InvokeAsync(action).DoNotWait();

#pragma warning disable 67
        public event EventHandler<EventArgs> Started;
        public event EventHandler<EventArgs> Idle;
        public event EventHandler<EventArgs> Terminating;
#pragma warning restore 67
        public bool IsUnitTestEnvironment => true;

        #region IMainThread
        public int ThreadId => UIThreadHelper.Instance.Thread.ManagedThreadId;

        public void Post(Action action, CancellationToken cancellationToken) =>
            UIThreadHelper.Instance.InvokeAsync(action, cancellationToken).DoNotWait();
        #endregion

        #region IIdleTimeSource
        public void DoIdle() {
            UIThreadHelper.Instance.Invoke(() => Idle?.Invoke(null, EventArgs.Empty));
            DoEvents();
        }

        public void DoEvents() {
            var disp = GetDispatcher();
            if (disp != null) {
                DispatcherFrame frame = new DispatcherFrame();
                disp.BeginInvoke(DispatcherPriority.Background,
                        new DispatcherOperationCallback(ExitFrame), frame);
                Dispatcher.PushFrame(frame);
            }
        }

        public object ExitFrame(object f) {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }

        private Dispatcher GetDispatcher(Thread thread = null) {
            if (thread == null) {
                if (_creatorThread != null && _creatorThread.ManagedThreadId == UIThreadHelper.Instance.Thread.ManagedThreadId) {
                    return Dispatcher.FromThread(_creatorThread);
                }
            } else {
                return Dispatcher.FromThread(thread);
            }
            return null;
        }
        #endregion
    }
}