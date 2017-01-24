// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.Common.Core.UI.Commands {
    /// <summary>
    /// An object that implements a single command. 
    /// </summary>
    public interface IAsyncCommand {
        /// <summary>
        /// Determines current command status.
        /// </summary>
        CommandStatus Status { get; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        Task InvokeAsync();
    }
}
