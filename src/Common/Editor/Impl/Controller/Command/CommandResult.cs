using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Languages.Editor
{
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    [DebuggerDisplay("Status={Status}, Result={Result}")]
    public struct CommandResult
    {
        public CommandStatus Status { get; set; }
        public int Result { get; set; }
        private const CommandStatus _successStatus = CommandStatus.Supported;

        /// <summary>
        /// If you are returning Executed/Disabled/Not Supported command results
        /// with generic result use the built in statics:
        /// CommandResult.Executed
        /// CommandResult.Disabled
        /// CommandResult.NotSupported
        /// </summary>
        /// <param name="status"></param>
        /// <param name="result"></param>
        public CommandResult(CommandStatus status, int result)
            : this()
        {
            Status = status;
            Result = result;
        }

        public static readonly CommandResult Executed = new CommandResult(CommandStatus.SupportedAndEnabled, 0);

        public static readonly CommandResult Disabled = new CommandResult(CommandStatus.Supported, 0);

        public static readonly CommandResult NotSupported = new CommandResult(CommandStatus.NotSupported, 0);

        /// <summary>
        /// returns true if the result._status was both Enabled and Supported
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool WasExecuted
        {
            get
            {
                return ((Status & _successStatus) == _successStatus);
            }
        }
    }
}
