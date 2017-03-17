// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Core.UI.Commands;

namespace Microsoft.Languages.Editor.Test.Shell {
    public sealed class TestUIServices: IUIServices {
        public TestUIServices() {

        }

        public UIColorTheme UIColorTheme => UIColorTheme.Dark;
        public IProgressDialog ProgressDialog { get; } = new TestProgressDialog();
        public IFileDialog FileDialog { get; } = new TestFileDialog();

#pragma warning disable 0067
        public event EventHandler<EventArgs> UIThemeChanged;
#pragma warning restore 0067

        public string SaveFileIfDirty(string fullPath) => throw new NotImplementedException();
        public void ShowContextMenu(CommandId commandId, int x, int y, object commandTarget = null) => throw new NotImplementedException();
        public void ShowErrorMessage(string message) => throw new NotImplementedException();
        public MessageButtons ShowMessage(string message, MessageButtons buttons, MessageType messageType = MessageType.Information) => throw new NotImplementedException();
        public void UpdateCommandStatus(bool immediate = false) => throw new NotImplementedException();
    }
}
