using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Support.Test.Functions {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FunctionIndexTest : UnitTestBase {
        [TestMethod]
        [TestCategory("R.Signatures")]
        public void FunctionInfoTest1() {
            FunctionIndexTestExecutor.ExecuteTest((ManualResetEventSlim evt) => {
                object result = FunctionIndex.GetFunctionInfo("abs", (object o) => {
                    FunctionInfoTest1_TestBody(evt);
                });

                if (result != null && !evt.IsSet) {
                    FunctionInfoTest1_TestBody(evt);
                }
            });
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void FunctionInfoTest2() {
            FunctionIndexTestExecutor.ExecuteTest((ManualResetEventSlim evt) => {
                object result = FunctionIndex.GetFunctionInfo("eval", (object o) => {
                    FunctionInfoTest2_TestBody(evt);
                });

                if (result != null && !evt.IsSet) {
                    FunctionInfoTest2_TestBody(evt);
                }
            });
        }

        private void FunctionInfoTest1_TestBody(ManualResetEventSlim completed) {
            IFunctionInfo functionInfo = FunctionIndex.GetFunctionInfo("abs");
            Assert.IsNotNull(functionInfo);

            Assert.AreEqual("abs", functionInfo.Name);
            Assert.IsTrue(functionInfo.Description.Length > 0);

            Assert.AreEqual(1, functionInfo.Signatures.Count);
            Assert.AreEqual(1, functionInfo.Signatures[0].Arguments.Count);

            List<int> locusPoints = new List<int>();
            Assert.AreEqual("abs(x)", functionInfo.Signatures[0].GetSignatureString(locusPoints));

            Assert.AreEqual(2, locusPoints.Count);
            Assert.AreEqual(4, locusPoints[0]);
            Assert.AreEqual(5, locusPoints[1]);

            completed.Set();
        }

        private void FunctionInfoTest2_TestBody(ManualResetEventSlim completed) {
            IFunctionInfo functionInfo = FunctionIndex.GetFunctionInfo("eval");
            Assert.IsNotNull(functionInfo);

            Assert.AreEqual("eval", functionInfo.Name);
            Assert.IsTrue(functionInfo.Description.Length > 0);

            Assert.AreEqual(1, functionInfo.Signatures.Count);
            Assert.AreEqual(3, functionInfo.Signatures[0].Arguments.Count);

            List<int> locusPoints = new List<int>();
            string signature = functionInfo.Signatures[0].GetSignatureString(locusPoints);
            Assert.AreEqual("eval(expr, envir = parent.frame(), enclos = if(is.list(envir) || is.pairlist(envir)) parent.frame() else baseenv())", signature);

            Assert.AreEqual(4, locusPoints.Count);
            Assert.AreEqual(5, locusPoints[0]);
            Assert.AreEqual(11, locusPoints[1]);
            Assert.AreEqual(35, locusPoints[2]);
            Assert.AreEqual(114, locusPoints[3]);

            completed.Set();
        }
    }
}
