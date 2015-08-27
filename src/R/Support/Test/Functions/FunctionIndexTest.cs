using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Support.Test.Functions
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FunctionIndexTest : UnitTestBase
    {
        [TestMethod]
        public void FunctionInfoTest1()
        {
            FunctionIndexTestExecutor.ExecuteTest((ManualResetEventSlim evt) =>
            {
                object result = FunctionIndex.GetFunctionInfo("abs", (object o) =>
                {
                    FunctionInfoTest1_TestBody01(evt);
                });

                if (result != null && !evt.IsSet)
                {
                    FunctionInfoTest1_TestBody01(evt);
                }
            });
        }

        private void FunctionInfoTest1_TestBody01(ManualResetEventSlim completed)
        {
            IFunctionInfo functionInfo = FunctionIndex.GetFunctionInfo("abs");
            Assert.IsNotNull(functionInfo);

            Assert.AreEqual("abs", functionInfo.Name);
            Assert.IsTrue(functionInfo.Description.Length > 0);

            Assert.AreEqual(2, functionInfo.Aliases.Count);
            Assert.AreEqual("abs", functionInfo.Aliases[0]);
            Assert.AreEqual("sqrt", functionInfo.Aliases[1]);

            Assert.AreEqual(1, functionInfo.Signatures.Count);
            Assert.AreEqual(1, functionInfo.Signatures[0].Arguments.Count);

            List<int> locusPoints = new List<int>();
            Assert.AreEqual("abs(x)", functionInfo.Signatures[0].GetSignatureString("abs", locusPoints));

            Assert.AreEqual(2, locusPoints.Count);
            Assert.AreEqual(4, locusPoints[0]);
            Assert.AreEqual(5, locusPoints[1]);

            completed.Set();
        }
    }
}
