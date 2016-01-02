using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Test.Controls;
using Microsoft.Common.Core.Test.STA;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.VisualStudio.R.Interactive.Test.Utility;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Application.Test.Data {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class VaraibleExplorerTest : UnitTestBase {
        [TestMethod]
        [TestCategory("Interactive")]
        public void VaraibleExplorer_ConstructorTest01() {
            using (var script = new ControlTestScript(typeof(VariableGridHost))) {
                string actual = script.WriteVisualTree();
                ViewTreeDump.CompareVisualTrees(this.TestContext, actual, "VariableExplorer01");
            }
        }

        [TestMethod]
        [TestCategory("Interactive")]
        public void VaraibleExplorer_ConstructorTest02() {
            using (var script = new ControlTestScript(typeof(VariableView))) {
                string actual = script.WriteVisualTree();
                ViewTreeDump.CompareVisualTrees(this.TestContext, actual, "VariableExplorer02");
            }
        }

        [TestMethod]
        [TestCategory("Interactive")]
        public void VaraibleExplorer_SimpleDataTest() {
            string actual = null;
            using (var hostScript = new RHostScript()) {
                using (var script = new ControlTestScript(typeof(VariableView))) {
                    DoIdle(100);
                    Task.Run(async () => {
                        using (var eval = await hostScript.Session.BeginEvaluationAsync()) {
                            await eval.EvaluateAsync("x <- c(1:10)");
                        }
                    }).Wait();

                    DoIdle(2000);
                    actual = script.WriteVisualTree();
                }
            }
            ViewTreeDump.CompareVisualTrees(this.TestContext, actual, "VariableExplorer03");
        }

        private static void DoIdle(int ms) {
            StaThread.Invoke(() => {
                int time = 0;
                while (time < ms) {
                    IdleTime.DoIdle();
                    Thread.Sleep(20);
                    time += 20;
                }
            });
        }
    }
}
