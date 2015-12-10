using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Actions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Actions.Test.Logging
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class LoggerTest
    {
        [TestMethod]
        [TestCategory("Logging")]
        public void Logging_NullLogTest()
        {
            IActionLinesLog log = new NullLog();
            log.WriteAsync(MessageCategory.Error, "message").Wait();
            log.WriteFormatAsync(MessageCategory.Error, "message").Wait();
            log.WriteLineAsync(MessageCategory.Error, "message").Wait();

            Assert.AreEqual(0, log.Content.Length);
            Assert.AreEqual(0, log.Lines.Count);
        }

        [TestMethod]
        [TestCategory("Logging")]
        public void Logging_LinesLogTest()
        {
            IActionLinesLog log = new LinesLog(NullLogWriter.Instance);

            log.WriteAsync(MessageCategory.Error, "message1").Wait();
            log.WriteLineAsync(MessageCategory.Error, " message2").Wait();
            log.WriteFormatAsync(MessageCategory.Error, "message3 {0}\r\n", 1).Wait();
            log.WriteLineAsync(MessageCategory.Error, "message4").Wait();

            Assert.AreEqual("message1 message2\r\nmessage3 1\r\nmessage4\r\n", log.Content);
            Assert.AreEqual(4, log.Lines.Count);
            Assert.AreEqual("message1 message2", log.Lines[0]);
            Assert.AreEqual("message3 1", log.Lines[1]);
            Assert.AreEqual("message4", log.Lines[2]);
            Assert.AreEqual(string.Empty, log.Lines[3]);
        }
    }
}
