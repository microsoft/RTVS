using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Languages.Editor.Controller.Command {
    /// <summary>
    /// Command identifier
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    [ExcludeFromCodeCoverage]
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
