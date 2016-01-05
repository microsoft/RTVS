using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Test.Utility;
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
        public void GetRdFunctionArgumentsTest01() {
            string rdData = @"\alias{abind}
\usage{
    abind(..., along=N, rev.along=NULL, new.names='abc', force.array=TRUE,
      make.names=use.anon.names, use.anon.names=1.75*(2/3),
      use.first.dimnames=FALSE, hier.names=FALSE, use.dnns=
      FALSE)
}";
            IReadOnlyList<IFunctionInfo> functionInfos = RdParser.GetFunctionInfos(rdData);

            Assert.IsNotNull(functionInfos);
            Assert.AreEqual(1, functionInfos.Count);

            IFunctionInfo functionInfo = functionInfos[0];

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
        public void GetRdFunctionArgumentsTest02() {
            string rdData = @"
\usage{
matrix(data = NA, nrow = 1, ncol = 1, byrow = FALSE,
       dimnames = NULL)

as.matrix(x, \dots)
\method{as.matrix}{data.frame}(x, rownames.force = NA, \dots)

is.matrix(x)
}";
            IReadOnlyList<IFunctionInfo> functionInfos = RdParser.GetFunctionInfos(rdData);

            Assert.IsNotNull(functionInfos);
            Assert.AreEqual(3, functionInfos.Count);

            Assert.AreEqual("matrix", functionInfos[0].Name);
            Assert.AreEqual("as.matrix", functionInfos[1].Name);
            Assert.AreEqual("is.matrix", functionInfos[2].Name);

            Assert.AreEqual(1, functionInfos[0].Signatures.Count);
            Assert.AreEqual(1, functionInfos[1].Signatures.Count);
            Assert.AreEqual(1, functionInfos[2].Signatures.Count);

            Assert.AreEqual(5, functionInfos[0].Signatures[0].Arguments.Count);
            Assert.AreEqual("nrow", functionInfos[0].Signatures[0].Arguments[1].Name);
            Assert.AreEqual("1", functionInfos[0].Signatures[0].Arguments[1].DefaultValue);
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void GetRdFunctionArgumentsDescriptionsTest01() {
            string rdData = @"\alias{abind}
\usage {
    abind(..., along=N, rev.along=NULL, new.names='abc')
}
\arguments{
  \item{\dots}{ Any number of vectors }

\item{along}{ (optional) The dimension along which to bind the arrays.
  }
}
";
            IReadOnlyList<IFunctionInfo> functionInfos = RdParser.GetFunctionInfos(rdData);

            Assert.IsNotNull(functionInfos);
            Assert.AreEqual(1, functionInfos.Count);

            IFunctionInfo functionInfo = functionInfos[0];
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
            IReadOnlyList<IFunctionInfo> functionInfos = RdParser.GetFunctionInfos(rdData);

            Assert.IsNotNull(functionInfos);
            Assert.AreEqual(2, functionInfos.Count);

            IFunctionInfo functionInfo = functionInfos[0];
            Assert.AreEqual("abs", functionInfo.Name);

            Assert.AreEqual("abs(x) computes the absolute value of x, sqrt(x) computes the (principal) square root of x, x. The naming follows the standard for computer languages such as C or Fortran.",
                functionInfo.Description);

            Assert.AreEqual(1, functionInfo.Signatures.Count);
            Assert.AreEqual(1, functionInfo.Signatures[0].Arguments.Count);
            Assert.AreEqual("a numeric or complex vector or array.", functionInfo.Signatures[0].Arguments[0].Description);
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void GetRdFunctionInfoTest02() {
            string rdData = TestFiles.LoadFile(this.TestContext, @"Help\02.rd");
            IReadOnlyList<IFunctionInfo> functionInfos = RdParser.GetFunctionInfos(rdData);

            Assert.IsNotNull(functionInfos);
            Assert.AreEqual(7, functionInfos.Count);

            Assert.AreEqual("lockEnvironment", functionInfos[0].Name);
            Assert.AreEqual("environmentIsLocked", functionInfos[1].Name);
            Assert.AreEqual("lockBinding", functionInfos[2].Name);
            Assert.AreEqual("unlockBinding", functionInfos[3].Name);
            Assert.AreEqual("bindingIsLocked", functionInfos[4].Name);
            Assert.AreEqual("makeActiveBinding", functionInfos[5].Name);
            Assert.AreEqual("bindingIsActive", functionInfos[6].Name);

            foreach(IFunctionInfo info in functionInfos) {
                Assert.AreEqual(1, info.Signatures.Count);

            Assert.AreEqual(
                "These functions represent an experimental interface for adjustments to environments and bindings within environments. They allow for locking environments as well as individual bindings, and for linking a variable to a function.",
                info.Description);
            }

            Assert.AreEqual(2, functionInfos[0].Signatures[0].Arguments.Count);
            Assert.AreEqual(1, functionInfos[1].Signatures[0].Arguments.Count);
            Assert.AreEqual(2, functionInfos[2].Signatures[0].Arguments.Count);
            Assert.AreEqual(2, functionInfos[3].Signatures[0].Arguments.Count);
            Assert.AreEqual(2, functionInfos[4].Signatures[0].Arguments.Count);
            Assert.AreEqual(3, functionInfos[5].Signatures[0].Arguments.Count);
            Assert.AreEqual(2, functionInfos[6].Signatures[0].Arguments.Count);

            Assert.AreEqual("an environment.", functionInfos[0].Signatures[0].Arguments[0].Description);
            Assert.AreEqual("logical specifying whether bindings should be locked.", functionInfos[0].Signatures[0].Arguments[1].Description);
            Assert.AreEqual("a name object or character string.", functionInfos[2].Signatures[0].Arguments[0].Description);
            Assert.AreEqual("a function taking zero or one arguments.", functionInfos[5].Signatures[0].Arguments[1].Description);
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void GetRdFunctionArgumentsBadData01() {
            IReadOnlyList<IFunctionInfo> functionInfos = RdParser.GetFunctionInfos(string.Empty);

            Assert.IsNotNull(functionInfos);
            Assert.AreEqual(0, functionInfos.Count);
        }
    }
}
