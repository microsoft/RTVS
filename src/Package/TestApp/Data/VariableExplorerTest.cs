using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Common.Core.Test.Controls;
using Microsoft.R.Host.Client;
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
        public void VariableExplorer_ConstructorTest02() {
            using (var hostScript = new VsRHostScript()) {
                using (var script = new ControlTestScript(typeof(VariableView))) {
                    var actual = VisualTreeObject.Create(script.Control);
                    ViewTreeDump.CompareVisualTrees(_files, actual, "VariableExplorer02");
                }
            }
        }

        [Test]
        [Category.Interactive]
        public void VariableExplorer_SimpleDataTest() {
            VisualTreeObject actual = null;
            using (var hostScript = new VsRHostScript()) {
                using (var script = new ControlTestScript(typeof(VariableView))) {
                    DoIdle(100);
                    Task.Run(async () => {
                        using (var eval = await hostScript.Session.BeginEvaluationAsync()) {
                            await eval.EvaluateAsync("x <- c(1:10)", REvaluationKind.Normal);
                        }
                    }).Wait();

                    DoIdle(2000);
                    actual = VisualTreeObject.Create(script.Control);
                }
            }
            ViewTreeDump.CompareVisualTrees(_files, actual, "VariableExplorer03");
        }
    }
}
