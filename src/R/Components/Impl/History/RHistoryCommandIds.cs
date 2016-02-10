using System;
using System.ComponentModel.Design;

namespace Microsoft.R.Components.History {
    public static class RHistoryCommandIds {
        private const string RCmdSetGuidString = "AD87578C-B324-44DC-A12A-B01A6ED5C6E3";
        private static readonly Guid RCmdSetGuid = new Guid(RCmdSetGuidString);

        public static CommandID ContextMenu { get; } = new CommandID(RCmdSetGuid, 102);
    }
}
