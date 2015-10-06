using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Parser
{
    /// <summary>
    /// Test the result by running DPUT on various types of data.
    /// </summary>
    /// <remarks>
    /// Separate test here is for testing logic used by data inspect like variable viewers
    /// </remarks>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParseDputResultTest
    {
        [TestMethod]
        public void ParseCharacterVectorTest()
        {
            string expression =
@"c(""one"", ""two"", ""three"")";
            AstRoot astRoot = RParser.Parse(expression);
            Assert.Inconclusive();
        }
    }
}
