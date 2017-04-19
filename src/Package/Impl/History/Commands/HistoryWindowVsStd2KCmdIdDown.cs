// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.History;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal class HistoryWindowVsStd2KCmdIdDown : NavigationCommandBase {
        public HistoryWindowVsStd2KCmdIdDown(ITextView textView, IRHistoryProvider historyProvider) 
            : base(textView, historyProvider, VSConstants.VSStd2KCmdID.DOWN) {}

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            History.SelectNextHistoryEntry();
            return CommandResult.Executed;
        }
    }
}