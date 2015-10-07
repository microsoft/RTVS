using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Actions.Logging
{
    public sealed class NullLog : IActionLinesLog
    {
        public Task WriteAsync(MessageCategory category, string message)
        {
            return Task.CompletedTask;
        }

        public Task WriteFormatAsync(MessageCategory category, string format, params object[] arguments)
        {
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(MessageCategory category, string message)
        {
            return Task.CompletedTask;
        }

        public string Content
        {
            get { return string.Empty; }
        }

        public IReadOnlyList<string> Lines
        {
            get { return new string[0]; }
        }
    }
}
