// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Common.Core.UI.Commands {
    /// <summary>
    /// An object that represents a command
    /// </summary>
    public interface ICommand : ICommandTarget {
        /// <summary>
        /// True if command requires file to be checked out from Source Code Control before execution
        /// </summary>
        bool NeedCheckout(Guid group, int id);

        /// <summary>
        /// List of command identifiers this class is handling
        /// </summary>
        IList<CommandId> CommandIds { get; }
    }
}
