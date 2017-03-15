// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.UI.Commands {
    /// <summary>
    /// Command identifier
    /// </summary>
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
        public Guid Group { get; set; }

        /// <summary>
        /// Command identifier within the group
        /// </summary>
        public int Id { get; set; }
    }
}
