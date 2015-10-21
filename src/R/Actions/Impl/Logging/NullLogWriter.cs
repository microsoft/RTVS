using System.Threading.Tasks;

namespace Microsoft.R.Actions.Logging {
    public sealed class NullLogWriter : IActionLogWriter {
        public static IActionLogWriter Instance { get; } = new NullLogWriter();

        private NullLogWriter() {
            
        }

        public Task WriteAsync(MessageCategory category, string message) {
            return Task.CompletedTask;
        }
    }
}