// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Settings;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Components.Controller;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NSubstitute;

namespace Microsoft.Languages.Editor.Test.Fakes.Shell {
    public class TestEditorShell : IEditorShell, IMainThread {
        private readonly CompositionContainer _container;

        public TestEditorShell(CompositionContainer container) {
            _container = container;
        }

        public ExportProvider ExportProvider => _container;
        public ICompositionService CompositionService => _container;

        public void DispatchOnUIThread(Action action) {
            UIThreadHelper.Instance.Invoke(action);
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

        public void ShowProgressBar(Func<CancellationToken, Task> method, string waitMessage, int delayToShowDialogMs = 0)
            => UIThreadHelper.Instance.Invoke(() => method(CancellationToken.None)).GetAwaiter().GetResult();

        public TResult ShowProgressBar<TResult>(Func<CancellationToken, Task<TResult>> method, string waitMessage, int delayToShowDialogMs = 0)
            => UIThreadHelper.Instance.Invoke(() => method(CancellationToken.None)).GetAwaiter().GetResult();

        public void ShowProgressBar(Func<IProgress<ProgressDialogData>, CancellationToken, Task> method, string waitMessage, int totalSteps = 100, int delayToShowDialogMs = 0)
            => UIThreadHelper.Instance.Invoke(() => method(new Progress<ProgressDialogData>(), CancellationToken.None)).GetAwaiter().GetResult();

        public T ShowProgressBar<T>(Func<IProgress<ProgressDialogData>, CancellationToken, Task<T>> method, string waitMessage, int totalSteps = 100, int delayToShowDialogMs = 0)
            => UIThreadHelper.Instance.Invoke(() => method(new Progress<ProgressDialogData>(), CancellationToken.None)).GetAwaiter().GetResult();

        public MessageButtons ShowMessage(string message, MessageButtons buttons) {
            LastShownMessage = message;
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
        public IApplicationConstants AppConstants => new TestAppConstants();
        public ICoreServices Services => TestCoreServices.CreateReal();
        public IWritableSettingsStorage SettingsStorage => Substitute.For<IWritableSettingsStorage>();

        #region IMainThread
        public int ThreadId => MainThread.ManagedThreadId;
        public void Post(Action action) => UIThreadHelper.Instance.InvokeAsync(action).DoNotWait();
        #endregion

        #region IEditorShell
        public ICommandTarget TranslateCommandTarget(ITextView textView, object commandTarget) {
            return commandTarget as ICommandTarget;
        }

        public object TranslateToHostCommandTarget(ITextView textView, object commandTarget) {
            return commandTarget;
        }

        public ICompoundUndoAction CreateCompoundAction(ITextView textView, ITextBuffer textBuffer) {
            return new CompoundUndoAction(textView, this, addRollbackOnCancel: false);
        }

        public bool IsUnitTestEnvironment { get; } = true;
        public void DoIdle() {
            UIThreadHelper.Instance.DoEvents();
            UIThreadHelper.Instance.Invoke(() => Idle?.Invoke(null, EventArgs.Empty));
        }
        #endregion
    }
}