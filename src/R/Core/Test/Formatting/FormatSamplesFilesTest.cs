using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Languages.Core.Test.Utility;
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
            FormatFilesFiles.FormatFile(this.TestContext, @"Formatting\lsfit.r");
        }
    }
}
