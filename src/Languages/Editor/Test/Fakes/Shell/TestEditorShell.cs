// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Design;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Components.Controller;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Test.Fakes.Shell {
    public class TestEditorShell : IEditorShell, IMainThread {
        private readonly CompositionContainer _container;

        public TestEditorShell(CompositionContainer container, ICoreServices coreServices) {
            FileDialog = new TestFileDialog();
            ProgressDialog = new TestProgressDialog();
            _container = container;
            Services = coreServices;
        }

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

        public void ShowErrorMessage(string message) {
            LastShownErrorMessage = message;
        }

        public void ShowContextMenu(CommandID commandId, int x, int y, object commandTaget = null) {
            LastShownContextMenu = commandId;
        }

        public MessageButtons ShowMessage(string message, MessageButtons buttons, MessageType messageType = MessageType.Information) {
            LastShownMessage = message;
            return MessageButtons.OK;
        }

        public string SaveFileIfDirty(string fullPath) => fullPath;

        public void UpdateCommandStatus(bool immediate) { }

        public int LocaleId => 1033;

        public string LastShownMessage { get; private set; }
        public string LastShownErrorMessage { get; private set; }
        public CommandID LastShownContextMenu { get; private set; }
        public IApplicationConstants AppConstants => new TestAppConstants();
        public IProgressDialog ProgressDialog { get; }
        public IFileDialog FileDialog { get; }
        public ICoreServices Services { get; }

        #region IMainThread
        public int ThreadId => MainThread.ManagedThreadId;
        public void Post(Action action, CancellationToken cancellationToken) => UIThreadHelper.Instance.InvokeAsync(action, cancellationToken).DoNotWait();
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