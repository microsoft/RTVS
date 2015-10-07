using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Actions.Logging
{
    public class StringLog : IActionLog
    {
        private StringBuilder _sb = new StringBuilder();

        public virtual Task WriteAsync(MessageCategory category, string message)
        {
            _sb.Append(message);
            return Task.CompletedTask;
        }

        public virtual Task WriteFormatAsync(MessageCategory category, string format, params object[] arguments)
        {
            string message = string.Format(CultureInfo.InvariantCulture, format, arguments);
            return WriteAsync(category, message);
        }

        public virtual Task WriteLineAsync(MessageCategory category, string message)
        {
            return WriteAsync(category, message + "\r\n");
        }

        public virtual string Content
        {
            get { return _sb.ToString(); }
        }
    }
}
