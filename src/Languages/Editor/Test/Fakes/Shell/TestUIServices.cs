// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Core.UI.Commands;

namespace Microsoft.Languages.Editor.Test.Fakes.Shell {
    public class TestUIServices : IUIService {
#pragma warning disable 67
        public event EventHandler<EventArgs> UIThemeChanged;
#pragma warning restore 67
        public UIColorTheme UIColorTheme => UIColorTheme.Light;
        public void ShowErrorMessage(string message)=> LastShownErrorMessage = message;
        public void ShowContextMenu(CommandId commandId, int x, int y, object commandTaget = null) => LastShownContextMenu = commandId;

        public MessageButtons ShowMessage(string message, MessageButtons buttons, MessageType messageType = MessageType.Information) {
            LastShownMessage = message;
            return MessageButtons.OK;
        }

        public string SaveFileIfDirty(string fullPath) => fullPath;
        public void UpdateCommandStatus(bool immediate) { }

        public string LastShownMessage { get; private set; }
        public string LastShownErrorMessage { get; private set; }
        public CommandId LastShownContextMenu { get; private set; }
        public IProgressDialog ProgressDialog { get; } = new TestProgressDialog();
        public IFileDialog FileDialog { get; } = new TestFileDialog();
    }
}