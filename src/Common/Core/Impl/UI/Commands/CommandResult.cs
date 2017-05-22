// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Common.Core.UI.Commands {
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    [DebuggerDisplay("Status={Status}, Result={Result}")]
    public struct CommandResult {
        public CommandStatus Status { get; set; }
        public int Result { get; set; }
        private const CommandStatus SuccessStatus = CommandStatus.Supported;

        /// <summary>
        /// If you are returning Executed/Disabled/Not Supported command results
        /// with generic result use the built in statics:
        /// CommandResult.Executed
        /// CommandResult.Disabled
        /// CommandResult.NotSupported
        /// </summary>
        /// <param name="status"></param>
        /// <param name="result"></param>
        public CommandResult(CommandStatus status, int result)
            : this() {
            Status = status;
            Result = result;
        }

        public static readonly CommandResult Executed = new CommandResult(CommandStatus.SupportedAndEnabled, 0);
        public static readonly CommandResult Disabled = new CommandResult(CommandStatus.Supported, 0);
        public static readonly CommandResult NotSupported = new CommandResult(CommandStatus.NotSupported, 0);

        /// <summary>
        /// returns true if the result._status was both Enabled and Supported
        /// </summary>
        /// <returns></returns>
        public bool WasExecuted => ((Status & SuccessStatus) == SuccessStatus);
    }
}
