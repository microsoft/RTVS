using System.Threading.Tasks;

namespace Microsoft.R.Actions.Logging
{
    /// <summary>
    /// Represents action logger. Log can be a text file,
    /// an application output window or telemetry.
    /// </summary>
    public interface IActionLog
    {
        Task WriteAsync(MessageCategory category, string message);
        Task WriteFormatAsync(MessageCategory category, string format, params object[] arguments);
        Task WriteLineAsync(MessageCategory category, string message);
        string Content { get; }
    }
}
