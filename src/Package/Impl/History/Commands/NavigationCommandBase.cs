// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Controllers.Commands;
using Microsoft.R.Components.History;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History.Commands {
    internal abstract class NavigationCommandBase : ViewCommand {
        protected IRHistory History { get; }

        protected NavigationCommandBase(ITextView textView, IRHistoryProvider historyProvider, VSConstants.VSStd2KCmdID id)
            : base(textView, VSConstants.VSStd2K, (int) id, false) {
            History = historyProvider.GetAssociatedRHistory(textView);
        }

        protected NavigationCommandBase(ITextView textView, IRHistoryProvider historyProvider, params VSConstants.VSStd2KCmdID[] ids)
            : base(textView, GetCommandIds(ids), false) {
            History = historyProvider.GetAssociatedRHistory(textView);
        }

        public override CommandStatus Status(Guid guid, int id) {
            return History.HasEntries
                ? CommandStatus.SupportedAndEnabled
                : CommandStatus.Supported;
        }

        private static CommandId[] GetCommandIds(VSConstants.VSStd2KCmdID[] ids) => ids.Select(id => new CommandId(VSConstants.VSStd2K, (int) id)).ToArray();
    }
}