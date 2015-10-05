using System.Threading.Tasks;

namespace Microsoft.R.Actions.Logging
{
    public interface IActionLog
    {
        Task WriteAsync(MessageCategory category, string message);
        Task WriteFormatAsync(MessageCategory category, string format, params object[] arguments);
        Task WriteLineAsync(MessageCategory category, string message);
        string Content { get; }
    }
}
