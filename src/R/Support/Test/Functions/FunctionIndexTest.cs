using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using FluentAssertions;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Support.Test.Functions {
    [ExcludeFromCodeCoverage]
    public class FunctionIndexTest {
        [Test(Skip = "Need to understand how test is working")]
        [Category.R.Signatures]
        public void FunctionInfoTest1() {
            FunctionIndexTestExecutor.ExecuteTest(evt => {
                object result = FunctionIndex.GetFunctionInfo("abs", o => {
                    FunctionInfoTest1_TestBody(evt);
                });

                if (result != null && !evt.IsSet) {
                    FunctionInfoTest1_TestBody(evt);
                }
            });
        }

        [Test(Skip = "Need to understand how test is working")]
        [Category.R.Signatures]
        public void FunctionInfoTest2() {
            FunctionIndexTestExecutor.ExecuteTest(evt => {
                object result = FunctionIndex.GetFunctionInfo("eval", o => {
                    FunctionInfoTest2_TestBody(evt);
                });

                if (result != null && !evt.IsSet) {
                    FunctionInfoTest2_TestBody(evt);
                }
            });
        }

        private void FunctionInfoTest1_TestBody(ManualResetEventSlim completed) {
            IFunctionInfo functionInfo = FunctionIndex.GetFunctionInfo("abs");
            functionInfo.Should().NotBeNull();
            functionInfo.Name.Should().Be("abs");
            functionInfo.Description.Should().NotBeEmpty();
            functionInfo.Signatures.Should().ContainSingle()
                .Which.Arguments.Should().ContainSingle();
            
            List<int> locusPoints = new List<int>();
            functionInfo.Signatures[0].GetSignatureString(locusPoints).Should().Be("abs(x)");
            locusPoints.Should().Equal(4, 5);

            completed.Set();
        }

        private void FunctionInfoTest2_TestBody(ManualResetEventSlim completed) {
            IFunctionInfo functionInfo = FunctionIndex.GetFunctionInfo("eval");
            functionInfo.Should().NotBeNull();
            functionInfo.Name.Should().Be("eval");
            functionInfo.Description.Should().NotBeEmpty();
            functionInfo.Signatures.Should().ContainSingle()
                .Which.Arguments.Should().HaveCount(3);

            List<int> locusPoints = new List<int>();
            string signature = functionInfo.Signatures[0].GetSignatureString(locusPoints);
            signature.Should().Be("eval(expr, envir = parent.frame(), enclos = if(is.list(envir) || is.pairlist(envir)) parent.frame() else baseenv())");
            locusPoints.Should().Equal(5, 11, 35, 114);
            
            completed.Set();
        }
    }
}
