// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Controls;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Interactive.Test.Utility;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.R.Package.Test;
using Xunit;

namespace Microsoft.VisualStudio.R.Interactive.Test.Data {
    [ExcludeFromCodeCoverage]
    [Category.Interactive]
    [Collection(CollectionNames.NonParallel)]
    public sealed class VariableExplorerTest : HostBasedInteractiveTest {
        private readonly TestFilesFixture _files;

        public VariableExplorerTest(IServiceContainer services, TestFilesFixture files): base(services) {
            _files = files;
        }

        [Test]
        public void ConstructorTest02() {
            using (var script = new ControlTestScript(typeof(VariableView), Services)) {
                var actual = VisualTreeObject.Create(script.Control);
                ViewTreeDump.CompareVisualTrees(_files, actual, "VariableExplorer02");
            }
        }

        [Test]
        public async Task SimpleDataTest() {
            VisualTreeObject actual = null;
            using (var script = new ControlTestScript(typeof(VariableView), Services)) {
                DoIdle(100);
                await HostScript.Session.ExecuteAsync("x <- c(1:10)");
                DoIdle(1000);
                actual = VisualTreeObject.Create(script.Control);
                ViewTreeDump.CompareVisualTrees(_files, actual, "VariableExplorer03");
            }
        }

        [Test]
        public async Task SimpleFunctionTest() {
            VisualTreeObject actual = null;
            using (var script = new ControlTestScript(typeof(VariableView), Services)) {
                DoIdle(100);
                await HostScript.Session.ExecuteAsync("x <- lm");
                DoIdle(1000);
                actual = VisualTreeObject.Create(script.Control);
                ViewTreeDump.CompareVisualTrees(_files, actual, "VariableExplorer04");
            }
        }
    }
}
