// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
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
    public sealed class VariableExplorerTest : InteractiveTest {
        private readonly TestFilesFixture _files;
        private readonly BrokerFixture _broker;

        public VariableExplorerTest(TestFilesFixture files, BrokerFixture brokerFixture) {
            _files = files;
            _broker = brokerFixture;
        }

        [Test]
        [Category.Interactive]
        public void ConstructorTest02() {
            using (var hostScript = new VsRHostScript(_sessionProvider, _broker.BrokerConnector)) {
                using (var script = new ControlTestScript(typeof(VariableView))) {
                    var actual = VisualTreeObject.Create(script.Control);
                    ViewTreeDump.CompareVisualTrees(_files, actual, "VariableExplorer02");
                }
            }
        }

        [Test]
        [Category.Interactive]
        public async Task SimpleDataTest() {
            VisualTreeObject actual = null;
            using (var hostScript = new VsRHostScript(_sessionProvider, _broker.BrokerConnector)) {
                using (var script = new ControlTestScript(typeof(VariableView))) {
                    DoIdle(100);
                    await hostScript.Session.ExecuteAsync("x <- c(1:10)");
                    DoIdle(1000);
                    actual = VisualTreeObject.Create(script.Control);
                }
            }
            ViewTreeDump.CompareVisualTrees(_files, actual, "VariableExplorer03");
        }

        [Test]
        [Category.Interactive]
        public void SimpleFunctionTest() {
            VisualTreeObject actual = null;
            using (var hostScript = new VsRHostScript(_sessionProvider, _broker.BrokerConnector)) {
                using (var script = new ControlTestScript(typeof(VariableView))) {
                    DoIdle(100);
                    Task.Run(async () => {
                        await hostScript.Session.ExecuteAsync("x <- lm");
                    }).Wait();

                    DoIdle(1000);
                    actual = VisualTreeObject.Create(script.Control);
                }
            }
            ViewTreeDump.CompareVisualTrees(_files, actual, "VariableExplorer04");
        }
    }
}
