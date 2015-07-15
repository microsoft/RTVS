
namespace Microsoft.R.Core.Parser
{
    /// <summary>
    /// Describes severity of the validation error
    /// </summary>
    public enum ErrorSeverity
    {
        /// <summary>
        /// Informational message, a suggestion
        /// </summary>
        Informational,
        /// <summary>
        /// Warnings such as obsolete constructs
        /// </summary>
        Warning,
        /// <summary>
        /// Syntax error
        /// </summary>
        Error,
        /// <summary>
        /// Fatal error, such as internal product error.
        /// </summary>
        Fatal
    }
}
