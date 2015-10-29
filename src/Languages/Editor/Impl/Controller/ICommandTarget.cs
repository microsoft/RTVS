using System;

namespace Microsoft.Languages.Editor.Controller {
    /// <summary>
    /// Command target: an object that can provide command status as well as execute commands
    /// </summary>
    public interface ICommandTarget {
        /// <summary>
        /// Determines current command status
        /// </summary>
        /// <param name="group">Command group</param>
        /// <param name="id">Command identifier</param>
        /// <returns>Command status</returns>
        CommandStatus Status(Guid group, int id);

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="group">Command group</param>
        /// <param name="id">Command identifier</param>
        /// <param name="inputArg">Input argument</param>
        /// <param name="outputArg">Output argument</param>
        /// <returns>Command result</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg);

        /// <summary>
        /// Allows post processing of a command after it has been handled
        /// </summary>
        /// <param name="result">Result of the executed command</param>
        /// <param name="group">Command group</param>
        /// <param name="id">Command identifier</param>
        /// <param name="inputArg">Input argument</param>
        /// <param name="outputArg">Output argument</param>
        /// <returns>Command result</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "4#")]
        void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg);
    }
}
