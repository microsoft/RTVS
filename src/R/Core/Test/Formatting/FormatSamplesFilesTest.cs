using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Tests.Utility;
using Microsoft.Languages.Core.Formatting;
using Microsoft.R.Core.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FormatSamplesFilesTest : UnitTestBase {
        [TestMethod]
        [TestCategory("R.Formatting")]
        public void Formatter_FormatFile_LeastSquares() {
            RFormatOptions options = new RFormatOptions();
            options.IndentType = IndentType.Tabs;

            FormatFilesFiles.FormatFile(this.TestContext, @"Formatting\lsfit.r", options);
        }

        [TestMethod]
        [TestCategory("R.Formatting")]
        public void Formatter_FormatFile_IfElse() {
            RFormatOptions options = new RFormatOptions();
            options.IndentSize = 2;

            FormatFilesFiles.FormatFile(this.TestContext, @"Formatting\ifelse.r", options);
        }
    }
}
