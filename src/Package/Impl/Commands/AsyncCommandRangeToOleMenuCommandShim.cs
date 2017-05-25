// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.UI.Commands;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal class AsyncCommandRangeToOleMenuCommandShim : PackageCommand {
        private readonly IAsyncCommandRange _commandRange;
        private readonly int _maxCount;

        public AsyncCommandRangeToOleMenuCommandShim(Guid group, int id, IAsyncCommandRange commandRange) : base(group, id) {
            Check.ArgumentNull(nameof(commandRange), commandRange);
            _commandRange = commandRange;
            _maxCount = commandRange.MaxCount;
        }
        
        protected override void SetStatus() {
            var index = MatchedCommandId - CommandID.ID;
            if (index >= _maxCount) {
                Visible = false;
                Enabled = false;
                MatchedCommandId = 0;
                return;
            }

            if (MatchedCommandId == 0) {
                index = 0;
            } 

            var status = _commandRange.GetStatus(index);

            Supported = status.HasFlag(CommandStatus.Supported);
            Enabled = status.HasFlag(CommandStatus.Enabled);
            Visible = !status.HasFlag(CommandStatus.Invisible);
            Checked = status.HasFlag(CommandStatus.Latched);

            if (Visible) {
                Text = _commandRange.GetText(index);
            }

            MatchedCommandId = 0;
        }

        protected override void Handle(object inArg, out object outArg) {
            outArg = null;
            if (Checked) {
                return;
            }

            var index = MatchedCommandId == 0 ? 0 : MatchedCommandId - CommandID.ID;
            if (index < 0 || index >= _maxCount) {
                MatchedCommandId = 0;
                return;
            }

            _commandRange.InvokeAsync(index).DoNotWait();
        }

        public override bool DynamicItemMatch(int cmdId) {
            var index = cmdId - CommandID.ID;

            if (index >= 0 && index < _maxCount) {
                MatchedCommandId = cmdId;
                return true;
            }

            MatchedCommandId = 0;
            return false;
        }
    }
}