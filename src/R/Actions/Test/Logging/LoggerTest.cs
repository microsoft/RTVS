using Microsoft.R.Actions.Logging;
using Xunit;

namespace Microsoft.R.Actions.Test.Logging {
    public class LoggerTest {
        [Fact]
        [Trait("Logging", "")]
        public void Logging_NullLogTest() {
            IActionLinesLog log = new NullLog();
            log.WriteAsync(MessageCategory.Error, "message").Wait();
            log.WriteFormatAsync(MessageCategory.Error, "message").Wait();
            log.WriteLineAsync(MessageCategory.Error, "message").Wait();

            Assert.Equal(0, log.Content.Length);
            Assert.Equal(0, log.Lines.Count);
        }

        [Fact]
        [Trait("Logging", "")]
        public void Logging_LinesLogTest() {
            IActionLinesLog log = new LinesLog(NullLogWriter.Instance);

            log.WriteAsync(MessageCategory.Error, "message1").Wait();
            log.WriteLineAsync(MessageCategory.Error, " message2").Wait();
            log.WriteFormatAsync(MessageCategory.Error, "message3 {0}\r\n", 1).Wait();
            log.WriteLineAsync(MessageCategory.Error, "message4").Wait();

            Assert.Equal("message1 message2\r\nmessage3 1\r\nmessage4\r\n", log.Content);
            Assert.Equal(4, log.Lines.Count);
            Assert.Equal("message1 message2", log.Lines[0]);
            Assert.Equal("message3 1", log.Lines[1]);
            Assert.Equal("message4", log.Lines[2]);
            Assert.Equal(string.Empty, log.Lines[3]);
        }
    }
}
