// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    [ExcludeFromCodeCoverage]
    public class TestCoreShell : ICoreShell, IMainThread {
        private readonly CompositionContainer _container;
        private readonly TestServiceManager _serviceManager;

        public TestCoreShell(CompositionContainer container, ICoreServices services) {
            _container = container;
            Services = services;
            _serviceManager = new TestServiceManager(container);
        }

        public IServiceContainer GlobalServices => _serviceManager;

        public ExportProvider ExportProvider => _container;
        public ICompositionService CompositionService => _container;

        public void DispatchOnUIThread(Action action) {
            UIThreadHelper.Instance.InvokeAsync(action).DoNotWait();
        }

        public Thread MainThread => UIThreadHelper.Instance.Thread;

#pragma warning disable 67
        public event EventHandler<EventArgs> Started;
        public event EventHandler<EventArgs> Idle;
        public event EventHandler<EventArgs> Terminating;
        public event EventHandler<EventArgs> UIThemeChanged;

        public void ShowErrorMessage(string message) {
            LastShownErrorMessage = message;
        }

        public void ShowContextMenu(System.ComponentModel.Design.CommandID commandId, int x, int y, object commandTaget = null) => LastShownContextMenu = commandId;

        public MessageButtons ShowMessage(string message, MessageButtons buttons, MessageType messageType = MessageType.Information) {
            LastShownMessage = message;
            if (buttons == MessageButtons.YesNo || buttons == MessageButtons.YesNoCancel) {
                return MessageButtons.Yes;
            }
            return MessageButtons.OK;
        }

        public string SaveFileIfDirty(string fullPath) => fullPath;

        public void UpdateCommandStatus(bool immediate) { }

        public int LocaleId => 1033;
        public string LastShownMessage { get; private set; }
        public string LastShownErrorMessage { get; private set; }
        public System.ComponentModel.Design.CommandID LastShownContextMenu { get; private set; }
        public bool IsUnitTestEnvironment => true;
        public IFileDialog FileDialog { get; } = new TestFileDialog();
        public IProgressDialog ProgressDialog { get; } = new TestProgressDialog();
        public IApplicationConstants AppConstants { get; } = new TestAppConstants();
        public ICoreServices Services { get; }

        #region IMainThread

        public int ThreadId => Services.MainThread.ThreadId;

        public void Post(Action action, CancellationToken cancellationToken) =>
            Services.MainThread.Post(action, cancellationToken);

        #endregion
    }
}