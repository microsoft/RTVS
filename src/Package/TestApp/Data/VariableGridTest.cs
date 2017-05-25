// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Controls;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Interactive.Test.Utility;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test;
using Microsoft.VisualStudio.R.Package.Test.DataInspect;
using Xunit;

namespace Microsoft.VisualStudio.R.Interactive.Test.Data {
    [ExcludeFromCodeCoverage]
    [Category.Interactive]
    [Collection(CollectionNames.NonParallel)]
    public class VariableGridTest : InteractiveTest {
        private readonly TestFilesFixture _files;
        private readonly VariableRHostScript _hostScript;

        public VariableGridTest(IServiceContainer services, TestFilesFixture files): base(services) {
            _files = files;
            _hostScript = new VariableRHostScript(Services);
        }

        public override Task DisposeAsync() {
            _hostScript.Dispose();
            return base.DisposeAsync();
        }

        [Test]
        public async Task ConstructorTest() {
            using (var script = new ControlTestScript(typeof(VariableGridHost), Services)) {
                await PrepareControl(_hostScript, script, "grid.test <- matrix(1:10, 2, 5)");
                var actual = VisualTreeObject.Create(script.Control);
                ViewTreeDump.CompareVisualTrees(_files, actual, "VariableGrid02");
            }
        }

        [Test]
        public async Task SortTest01() {
            using (var script = new ControlTestScript(typeof(VariableGridHost), Services)) {
                await PrepareControl(_hostScript, script, "grid.test <- matrix(1:10, 2, 5)");
                var header = VisualTreeTestExtensions.FindFirstVisualChildOfType<HeaderTextVisual>(script.Control);
                var grid = VisualTreeTestExtensions.FindFirstVisualChildOfType<VisualGrid>(script.Control);
                header.Should().NotBeNull();
                await UIThreadHelper.Instance.InvokeAsync(() => {
                    grid.ToggleSort(header, false);
                    DoIdle(200);
                    grid.ToggleSort(header, false);
                });
                DoIdle(200);
                var actual = VisualTreeObject.Create(script.Control);
                ViewTreeDump.CompareVisualTrees(_files, actual, "VariableGridSorted01");
            }
        }

        [Test]
        public async Task SortTest02() {
            using (var script = new ControlTestScript(typeof(VariableGridHost), Services)) {
                await PrepareControl(_hostScript, script, "grid.test <- mtcars");
                UIThreadHelper.Instance.Invoke(() => {
                    var grid = VisualTreeTestExtensions.FindFirstVisualChildOfType<VisualGrid>(script.Control);

                    var header = VisualTreeTestExtensions.FindFirstVisualChildOfType<HeaderTextVisual>(script.Control); // mpg
                    header = VisualTreeTestExtensions.FindNextVisualSiblingOfType<HeaderTextVisual>(header); // cyl
                    header.Should().NotBeNull();

                    grid.ToggleSort(header, false);
                    DoIdle(200);

                    header = VisualTreeTestExtensions.FindNextVisualSiblingOfType<HeaderTextVisual>(header); // disp
                    header = VisualTreeTestExtensions.FindNextVisualSiblingOfType<HeaderTextVisual>(header); // hp

                    grid.ToggleSort(header, add: true);
                    grid.ToggleSort(header, add: true);
                    DoIdle(200);
                });

                var actual = VisualTreeObject.Create(script.Control);
                ViewTreeDump.CompareVisualTrees(_files, actual, "VariableGridSorted02");
            }
        }

        private async Task PrepareControl(VariableRHostScript hostScript, ControlTestScript script, string expression) {
            DoIdle(100);

            var result = await hostScript.EvaluateAsync(expression);
            var wrapper = new VariableViewModel(result, VsAppShell.Current.Services);

            DoIdle(2000);
            wrapper.Should().NotBeNull();

            UIThreadHelper.Instance.Invoke(() => {
                var host = (VariableGridHost)script.Control;
                host.SetEvaluation(wrapper);
            });

            DoIdle(1000);
        }
    }
}
