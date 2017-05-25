// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Common.Core.UI.Commands {
    /// <summary>
    /// Command identifier
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct CommandId {
        public CommandId(Guid group, int id)
            : this() {
            Group = group;
            Id = id;
        }

        public CommandId(int id)
            : this(Guid.Empty, id) {
        }

        /// <summary>
        /// Command group identifier
        /// </summary>
        public Guid Group { get;}

        /// <summary>
        /// Command identifier within the group
        /// </summary>
        public int Id { get; }
    }
}
