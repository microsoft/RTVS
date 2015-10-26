using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static System.FormattableString;

namespace Microsoft.R.Actions.Logging {
    public sealed class FileLogWriter : IActionLogWriter {
        private const int _maxBufferSize = 1024;
        private readonly char[] _lineBreaks = { '\n' };
        private readonly string _filePath;
        private readonly ActionBlock<string> _messages;
        private StringBuilder _sb = new StringBuilder();

        private async Task WriteToFile(string message) {
            try {
                // Writing every little thing via open/write/close
                // is expensive and slows down output to REPL quite a bit.
                _sb.Append(message);
                await FlushBuffer();
            } catch (UnauthorizedAccessException ex) {
                Trace.Fail(ex.ToString());
            } catch (PathTooLongException ex) {
                Trace.Fail(ex.ToString());
            } catch (DirectoryNotFoundException ex) {
                Trace.Fail(ex.ToString());
            } catch (NotSupportedException ex) {
                Trace.Fail(ex.ToString());
            } catch (IOException ex) {
                Trace.Fail(ex.ToString());
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            FlushBuffer(force: true).Wait();
        }

        private void OnProcessExit(object sender, EventArgs e) {
            FlushBuffer(force: true).Wait();
        }

        private async Task FlushBuffer(bool force = false) {
            if (_sb.Length > _maxBufferSize || force) {
                using (var stream = File.AppendText(_filePath)) {
                    await stream.WriteAsync(_sb.ToString());
                }
                _sb.Clear();
            }
        }

        public FileLogWriter(string filePath) {
            _filePath = filePath;
            _messages = new ActionBlock<string>(new Func<string, Task>(WriteToFile));

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        public Task WriteAsync(MessageCategory category, string message) {
            return _messages.SendAsync(GetStringToWrite(category, message));
        }

        public void Flush() {
            FlushBuffer(force: true).Wait();
        }


        private string GetStringToWrite(MessageCategory category, string message) {
            var categoryString = GetCategoryString(category);
            var prefix = Invariant($"[{DateTime.Now:yy-M-dd_HH-mm-ss}]{categoryString}:");
            if (!message.Take(message.Length - 1).Contains('\n')) {
                return prefix + message;
            }

            var emptyPrefix = new string(' ', prefix.Length);
            var lines = message.Split(_lineBreaks, StringSplitOptions.RemoveEmptyEntries)
                .Select((line, i) => i == 0 ? prefix + line + "\n" : emptyPrefix + line + "\n");
            return string.Concat(lines);
        }

        public static FileLogWriter InTempFolder(string fileName) {
            var path = $@"{Path.GetTempPath()}/{fileName}_{DateTime.Now:yyyyMdd_HHmmss}.log";
            return new FileLogWriter(path);
        }

        private static string GetCategoryString(MessageCategory category) {
            switch (category) {
                case MessageCategory.Error:
                    return "[ERROR]";
                case MessageCategory.Warning:
                    return "[WARNING]";
                default:
                    return string.Empty;
            }
        }
    }
}