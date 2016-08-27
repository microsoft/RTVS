// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.R.Components.Controller {
    public interface IAsyncCommandRange {
        /// <summary>
        /// Determines current command status.
        /// </summary>
        CommandStatus GetStatus(int index);

        /// <summary>
        /// Returns text for the command
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        string GetText(int index);

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="index"></param>
        Task<CommandResult> InvokeAsync(int index);

        /// <summary>
        /// Returns maximum index
        /// </summary>
        int MaxCount { get; }
    }
}