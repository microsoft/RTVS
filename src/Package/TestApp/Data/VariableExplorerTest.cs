using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.VisualStudio.R.Interactive.Test.Utility;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Application.Test.Data {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class VaraibleExplorerTest: UnitTestBase {
        [TestMethod]
        [TestCategory("Interactive")]
        public void VaraibleExplorer_ConstructorTest() {
            using (var script = new ControlTestScript(typeof(VariableGridHost))) {
                string actual = script.WriteVisualTree();
                ViewTreeDump.CompareVisualTrees(this.TestContext, actual, "VariableExplorer01");
            }
        }
    }
}
