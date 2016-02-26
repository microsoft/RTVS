using System;

namespace Microsoft.R.Actions.Logging {
    public static class GeneralLog {
        private static FileLogWriter _fileLogWriter;
        public static IActionLogWriter LogWriter {
            get {
                if(_fileLogWriter == null) {
                    _fileLogWriter = FileLogWriter.InTempFolder("Microsoft.R.General");
                }
                return _fileLogWriter;
            }
        }

        public static void Write(string message) {
            LogWriter.WriteAsync(MessageCategory.General, message + Environment.NewLine);
        }

        public static void Write(Exception ex) {
            LogWriter.WriteAsync(MessageCategory.Error, $"Exception {ex.Message}" + Environment.NewLine);
            LogWriter.WriteAsync(MessageCategory.Error, $"Stack trace {ex.StackTrace}" + Environment.NewLine);
        }
    }
}
