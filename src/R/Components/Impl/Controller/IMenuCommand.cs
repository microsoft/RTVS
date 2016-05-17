// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.Controller {
    /// <summary>
    /// An object that implements a single command. 
    /// </summary>
    public interface IMenuCommand {
        /// <summary>
        /// Determines current command status.
        /// </summary>
        CommandStatus Status { get; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        CommandResult Invoke(object inputArg, ref object outputArg);
    }
}
