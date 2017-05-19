// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Core.UI.Commands;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    public sealed class TestUIServices: IUIService {
#pragma warning disable 67
        public event EventHandler<EventArgs> UIThemeChanged;
#pragma warning restore 67

        public TestUIServices(IProgressDialog progressDialog) {
            ProgressDialog = progressDialog;
        }

        public void ShowErrorMessage(string message)=> LastShownErrorMessage = message;
        public void ShowContextMenu(CommandId commandId, int x, int y, object commandTaget = null) => LastShownContextMenu = commandId;

        public MessageButtons ShowMessage(string message, MessageButtons buttons, MessageType messageType = MessageType.Information) {
            LastShownMessage = message;
            if (buttons == MessageButtons.YesNo || buttons == MessageButtons.YesNoCancel) {
                return MessageButtons.Yes;
            }
            return MessageButtons.OK;
        }

        public IProgressDialog ProgressDialog { get; }
        public UIColorTheme UIColorTheme => UIColorTheme.Light;
        public IFileDialog FileDialog { get; } = new TestFileDialog();

        public string SaveFileIfDirty(string fullPath) => fullPath;
        public void UpdateCommandStatus(bool immediate) { }

        public string LastShownMessage { get; private set; }
        public string LastShownErrorMessage { get; private set; }
        public CommandId LastShownContextMenu { get; private set; }
    }
}
