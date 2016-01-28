using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using FluentAssertions;
using Microsoft.Common.Core.Test.Controls;
using Microsoft.R.Debugger;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Interactive.Test.Utility;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.R.Package.Test.DataInspect;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Xunit;

namespace Microsoft.VisualStudio.R.Interactive.Test.Data {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class VariableGridTest : InteractiveTest {
        private readonly TestFilesFixture _files;

        public VariableGridTest(TestFilesFixture files) {
            _files = files;
        }

        [Test]
        [Category.Interactive]
        public void VariableGrid_ConstructorTest01() {
            using (var script = new ControlTestScript(typeof(VariableGridHost))) {
                var actual = VisualTreeObject.Create(script.Control);
                ViewTreeDump.CompareVisualTrees(_files, actual, "VariableExplorer01");
            }
        }

        [Test]
        [Category.Interactive]
        public void VariableGrid_ConstructorTest02() {
            VisualTreeObject actual = null;
            using (var hostScript = new VariableRHostScript()) {
                using (var script = new ControlTestScript(typeof(VariableGridHost))) {
                    DoIdle(100);

                    EvaluationWrapper wrapper = null;
                    Task.Run(async () => {
                        hostScript.VariableProvider.Subscribe(
                            0,
                            "grid.test",
                            (r) => wrapper = new EvaluationWrapper(r));

                        await hostScript.EvaluateAsync("grid.test <- matrix(1:10, 2, 5)");
                    }).Wait();

                    DoIdle(2000);

                    wrapper.Should().NotBeNull();

                    UIThreadHelper.Instance.Invoke(() => {
                        var host = (VariableGridHost)script.Control;
                        host.SetEvaluation(wrapper);
                    });

                    DoIdle(1000);

                    actual = VisualTreeObject.Create(script.Control);
                }
            }
            ViewTreeDump.CompareVisualTrees(_files, actual, "VariableGrid02");
        }
    }
}
