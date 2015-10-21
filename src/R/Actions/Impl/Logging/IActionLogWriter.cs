using System.Threading.Tasks;

namespace Microsoft.R.Actions.Logging {
    public interface IActionLogWriter {
        Task WriteAsync(MessageCategory category, string message);
    }
}