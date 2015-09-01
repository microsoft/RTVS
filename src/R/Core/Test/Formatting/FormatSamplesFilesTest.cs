using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FormatSamplesFilesTest : UnitTestBase
    {
        [TestMethod]
        public void Formatter_FormatFile_LeastSquares()
        {
            RFormatOptions options = new RFormatOptions();
            options.IndentType = IndentType.Tabs;

            FormatFilesFiles.FormatFile(this.TestContext, @"Formatting\lsfit.r", options);
        }
    }
}
