using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.RD.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Support.Test.RD.Help {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class GetFunctionInfoTest : UnitTestBase {
        [TestMethod]
        [TestCategory("R.Signatures")]
        public void GetRdFunctionAliasesTest() {
            string rdData = @"\alias{abs}\alias{sqrt}";
            IFunctionInfo functionInfo = RdParser.GetFunctionInfo("abs", rdData);

            Assert.IsNotNull(functionInfo);
            Assert.AreEqual("abs", functionInfo.Name);

            Assert.AreEqual(2, functionInfo.Aliases.Count);
            Assert.AreEqual("abs", functionInfo.Aliases[0]);
            Assert.AreEqual("sqrt", functionInfo.Aliases[1]);
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void GetRdFunctionDescriptionTest() {
            string rdData = "\\description{\rA.  BC\r\n\t  EF}";
            IFunctionInfo functionInfo = RdParser.GetFunctionInfo("abs", rdData);

            Assert.IsNotNull(functionInfo);
            Assert.AreEqual("A. BC EF", functionInfo.Description);
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void GetRdFunctionArgumentsTest01() {
            string rdData = @"
\usage{
    abind(..., along=N, rev.along=NULL, new.names='abc', force.array=TRUE,
      make.names=use.anon.names, use.anon.names=1.75*(2/3),
      use.first.dimnames=FALSE, hier.names=FALSE, use.dnns=
      FALSE)
}";
            IFunctionInfo functionInfo = RdParser.GetFunctionInfo("abind", rdData);

            Assert.IsNotNull(functionInfo);
            Assert.AreEqual(1, functionInfo.Signatures.Count);
            Assert.AreEqual(10, functionInfo.Signatures[0].Arguments.Count);

            Assert.AreEqual("...", functionInfo.Signatures[0].Arguments[0].Name);

            Assert.AreEqual("along", functionInfo.Signatures[0].Arguments[1].Name);
            Assert.AreEqual("N", functionInfo.Signatures[0].Arguments[1].DefaultValue);

            Assert.AreEqual("rev.along", functionInfo.Signatures[0].Arguments[2].Name);
            Assert.AreEqual("NULL", functionInfo.Signatures[0].Arguments[2].DefaultValue);

            Assert.AreEqual("new.names", functionInfo.Signatures[0].Arguments[3].Name);
            Assert.AreEqual(@"'abc'", functionInfo.Signatures[0].Arguments[3].DefaultValue);

            Assert.AreEqual("force.array", functionInfo.Signatures[0].Arguments[4].Name);
            Assert.AreEqual("TRUE", functionInfo.Signatures[0].Arguments[4].DefaultValue);

            Assert.AreEqual("make.names", functionInfo.Signatures[0].Arguments[5].Name);
            Assert.AreEqual("use.anon.names", functionInfo.Signatures[0].Arguments[5].DefaultValue);

            Assert.AreEqual("use.anon.names", functionInfo.Signatures[0].Arguments[6].Name);
            Assert.AreEqual("1.75*(2/3)", functionInfo.Signatures[0].Arguments[6].DefaultValue);

            Assert.AreEqual("use.first.dimnames", functionInfo.Signatures[0].Arguments[7].Name);
            Assert.AreEqual("FALSE", functionInfo.Signatures[0].Arguments[7].DefaultValue);

            Assert.AreEqual("hier.names", functionInfo.Signatures[0].Arguments[8].Name);
            Assert.AreEqual("FALSE", functionInfo.Signatures[0].Arguments[8].DefaultValue);

            Assert.AreEqual("use.dnns", functionInfo.Signatures[0].Arguments[9].Name);
            Assert.AreEqual("FALSE", functionInfo.Signatures[0].Arguments[9].DefaultValue);
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void GetRdFunctionArgumentsDescriptionsTest01() {
            string rdData = @"
\usage {
    abind(..., along=N, rev.along=NULL, new.names='abc')
}
\arguments{
  \item{\dots}{ Any number of vectors }

\item{along}{ (optional) The dimension along which to bind the arrays.
  }
}
";
            IFunctionInfo functionInfo = RdParser.GetFunctionInfo("abind", rdData);

            Assert.IsNotNull(functionInfo);
            Assert.AreEqual(1, functionInfo.Signatures.Count);
            Assert.AreEqual(4, functionInfo.Signatures[0].Arguments.Count);

            Assert.AreEqual("...", functionInfo.Signatures[0].Arguments[0].Name);
            Assert.AreEqual("Any number of vectors", functionInfo.Signatures[0].Arguments[0].Description);

            Assert.AreEqual("along", functionInfo.Signatures[0].Arguments[1].Name);
            Assert.AreEqual("(optional) The dimension along which to bind the arrays.", functionInfo.Signatures[0].Arguments[1].Description);

            Assert.AreEqual("rev.along", functionInfo.Signatures[0].Arguments[2].Name);
            Assert.AreEqual("", functionInfo.Signatures[0].Arguments[2].Description);

            Assert.AreEqual("new.names", functionInfo.Signatures[0].Arguments[3].Name);
            Assert.AreEqual("", functionInfo.Signatures[0].Arguments[3].Description);
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void GetRdFunctionInfoTest01() {
            string rdData = TestFiles.LoadFile(this.TestContext, @"Help\01.rd");
            IFunctionInfo functionInfo = RdParser.GetFunctionInfo("abs", rdData);

            Assert.IsNotNull(functionInfo);
            Assert.AreEqual("abs", functionInfo.Name);

            Assert.AreEqual("abs(x) computes the absolute value of x, sqrt(x) computes the (principal) square root of x, x. The naming follows the standard for computer languages such as C or Fortran.",
                functionInfo.Description);

            Assert.AreEqual(2, functionInfo.Aliases.Count);
            Assert.AreEqual("abs", functionInfo.Aliases[0]);
            Assert.AreEqual("sqrt", functionInfo.Aliases[1]);

            Assert.AreEqual(1, functionInfo.Signatures.Count);
            Assert.AreEqual(1, functionInfo.Signatures[0].Arguments.Count);
            Assert.AreEqual("a numeric or complex vector or array.", functionInfo.Signatures[0].Arguments[0].Description);

        }
    }
}
