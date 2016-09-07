// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Common.Wpf.Threading;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.R.Components.Test.Fakes.Shell {
    [ExcludeFromCodeCoverage]
    public class TestCoreShell : ICoreShell, IMainThread {
        private readonly CompositionContainer _container;

        public TestCoreShell(CompositionContainer container) {
            _container = container;
        }

        public ExportProvider ExportProvider => _container;
        public ICompositionService CompositionService => _container;

        public void DispatchOnUIThread(Action action) {
            UIThreadHelper.Instance.Invoke(action);
        }

        public Task<TResult> DispatchOnMainThreadAsync<TResult>(Func<TResult> callback, CancellationToken cancellationToken = default(CancellationToken)) {
            return UIThreadHelper.Instance.InvokeAsync(callback);
        }

        public Thread MainThread => UIThreadHelper.Instance.Thread;

#pragma warning disable 67
        public event EventHandler<EventArgs> Idle;
        public event EventHandler<EventArgs> Terminating;

        public void ShowErrorMessage(string message) {
            LastShownErrorMessage = message;
        }

        public void ShowContextMenu(CommandID commandId, int x, int y, object commandTaget = null) {
            LastShownContextMenu = commandId;
        }

        public MessageButtons ShowMessage(string message, MessageButtons buttons) {
            LastShownMessage = message;
            if (buttons == MessageButtons.YesNo || buttons == MessageButtons.YesNoCancel) {
                return MessageButtons.Yes;
            }
            return MessageButtons.OK;
        }

        public string SaveFileIfDirty(string fullPath) => fullPath;

        public string ShowOpenFileDialog(string filter, string initialPath = null, string title = null) => OpenFilePath;

        public string ShowBrowseDirectoryDialog(string initialPath = null, string title = null) => BrowseDirectoryPath;

        public string ShowSaveFileDialog(string filter, string initialPath = null, string title = null) => SaveFilePath;

        public void UpdateCommandStatus(bool immediate) { }

        public int LocaleId => 1033;

        public string LastShownMessage { get; private set; }
        public string LastShownErrorMessage { get; private set; }
        public CommandID LastShownContextMenu { get; private set; }
        public string OpenFilePath { get; set; }
        public string BrowseDirectoryPath { get; set; }
        public string SaveFilePath { get; set; }
        public ITelemetryService TelemetryService { get; }

        public bool IsUnitTestEnvironment => true;

        public IntPtr ApplicationWindowHandle { get; }

        #region IMainThread
        public int ThreadId => MainThread.ManagedThreadId;
        public void Post(Action action) => UIThreadHelper.Instance.InvokeAsync(action).DoNotWait();
        #endregion
    }
}