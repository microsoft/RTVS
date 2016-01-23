using System;
using System.Collections.Generic;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.R.Components.Controller;

namespace Microsoft.Languages.Editor.Controller {
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
