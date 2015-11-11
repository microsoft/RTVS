using System;

namespace Microsoft.Languages.Editor.Controller {
    /// <summary>
    /// Command target that provides changing command names
    /// </summary>
    public interface IDynamicCommandTarget: ICommandTarget {
        /// <summary>
        /// Determines current command name
        /// </summary>
        /// <param name="group">Command group</param>
        /// <param name="id">Command identifier</param>
        /// <returns>Command name</returns>
        string GetCommandName(Guid group, int id);
    }
}
