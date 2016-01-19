using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Common.Core.Test.Controls;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Interactive.Test.Utility;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Xunit;

namespace Microsoft.VisualStudio.R.Interactive.Test.Data {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class VariableExplorerTest : InteractiveTest {
        private readonly TestFilesFixture _files;

        public VariableExplorerTest(TestFilesFixture files) {
            _files = files;
        }

        [Test]
        [Category.Interactive]
        public void VariableExplorer_ConstructorTest01() {
            using (var script = new ControlTestScript(typeof(VariableGridHost))) {
                string actual = script.WriteVisualTree();
                ViewTreeDump.CompareVisualTrees(_files, actual, "VariableExplorer01");
            }
        }

        [Test]
        [Category.Interactive]
        public void VariableExplorer_ConstructorTest02() {
            using (var hostScript = new VsRHostScript()) {
                using (var script = new ControlTestScript(typeof(VariableView))) {
                    string actual = script.WriteVisualTree();
                    ViewTreeDump.CompareVisualTrees(_files, actual, "VariableExplorer02");
                }
            }
        }

        [Test]
        [Category.Interactive]
        public void VariableExplorer_SimpleDataTest() {
            string actual = null;
            using (var hostScript = new VsRHostScript()) {
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
            ViewTreeDump.CompareVisualTrees(_files, actual, "VariableExplorer03");
        }
    }
}
