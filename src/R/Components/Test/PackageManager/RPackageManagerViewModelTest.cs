using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.PackageManager.ViewModel;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.Test.Fakes.InteractiveWindow;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;
using static Microsoft.UnitTests.Core.Threading.UIThreadTools;

namespace Microsoft.R.Components.Test.PackageManager {
    public class RPackageManagerViewModelTest : IAsyncLifetime {
        private readonly TestFilesFixture _testFiles;
        private readonly ExportProvider _exportProvider;
        private readonly MethodInfo _testMethod;
        private readonly IRInteractiveWorkflow _workflow;
        private IRPackageManagerVisualComponent _packageManagerComponent;
        private IRPackageManagerViewModel _packageManagerViewModel;


        public RPackageManagerViewModelTest(RComponentsMefCatalogFixture catalog, TestMethodFixture testMethod, TestFilesFixture testFiles) {
            _exportProvider = catalog.CreateExportProvider();
            _workflow = _exportProvider.GetExportedValue<TestRInteractiveWorkflowProvider>().GetOrCreate();
            _testMethod = testMethod.MethodInfo;
            _testFiles = testFiles;
        }

        public async Task InitializeAsync() {
            var settings = _exportProvider.GetExportedValue<IRSettings>();
            await _workflow.RSession.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name,
                RBasePath = settings.RBasePath,
                RHostCommandLineArguments = settings.RCommandLineArguments,
                CranMirrorName = settings.CranMirror,
            }, null, 50000);

            using (var eval = await _workflow.RSession.BeginEvaluationAsync()) {
                await TestRepositories.SetLocalRepoAsync(eval, _testFiles);
                await TestLibraries.SetLocalLibraryAsync(eval, _testMethod, _testFiles);
            }

            var componentContainerFactory = _exportProvider.GetExportedValue<IRPackageManagerVisualComponentContainerFactory>();
            _packageManagerComponent = await InUI(() => _workflow.Packages.GetOrCreateVisualComponent(componentContainerFactory));
            _packageManagerViewModel = await InUI(() => _packageManagerComponent.Control.DataContext) as IRPackageManagerViewModel;
        }

        public Task DisposeAsync() {
            _packageManagerComponent.Dispose();
            (_exportProvider as IDisposable)?.Dispose();
            return Task.CompletedTask;
        }

        [Test]
        public void ViewModelExists() {
            _packageManagerViewModel.Should().NotBeNull();
        }

        [Test]
        public async Task SwitchAvailableInstalledLoadedAsync() {
            var t1 = InUI(() => _packageManagerViewModel.SwitchToAvailablePackagesAsync());
            var t2 = InUI(() => _packageManagerViewModel.SwitchToInstalledPackagesAsync());
            var t3 = InUI(() => _packageManagerViewModel.SwitchToLoadedPackagesAsync());

            await Task.WhenAll(t1, t2, t3);

            _packageManagerViewModel.IsLoading.Should().BeFalse();
            _packageManagerViewModel.SelectedPackage.Should().NotBeNull();
            _packageManagerViewModel.Items.Should().OnlyContain(o => ((IRPackageViewModel) o).IsLoaded)
                .And.Contain(_packageManagerViewModel.SelectedPackage);
        }

        [Test]
        public async Task SwitchAvailableLoadedInstalledAsync() {
            var t1 = InUI(() => _packageManagerViewModel.SwitchToAvailablePackagesAsync());
            var t2 = InUI(() => _packageManagerViewModel.SwitchToLoadedPackagesAsync());
            var t3 = InUI(() => _packageManagerViewModel.SwitchToInstalledPackagesAsync());

            await Task.WhenAll(t1, t2, t3);

            _packageManagerViewModel.IsLoading.Should().BeFalse();
            _packageManagerViewModel.SelectedPackage.Should().NotBeNull();
            _packageManagerViewModel.Items.Should().OnlyContain(o => ((IRPackageViewModel) o).IsInstalled)
                .And.Contain(_packageManagerViewModel.SelectedPackage);
        }

        [Test]
        public async Task SwitchLoadedInstalledAvailableAsync() {
            var t1 = InUI(() => _packageManagerViewModel.SwitchToLoadedPackagesAsync());
            var t2 = InUI(() => _packageManagerViewModel.SwitchToInstalledPackagesAsync());
            var t3 = InUI(() => _packageManagerViewModel.SwitchToAvailablePackagesAsync());
            
            await Task.WhenAll(t1, t2, t3);
            var expected = new [] { "NotAvailable1", "NotAvailable2", "rtvslib1" };

            _packageManagerViewModel.IsLoading.Should().BeFalse();
            _packageManagerViewModel.SelectedPackage.Should().NotBeNull();
            _packageManagerViewModel.Items.Should().Equal(expected, (o, n) => ((IRPackageViewModel)o).Name.EqualsOrdinal(n))
                .And.Contain(_packageManagerViewModel.SelectedPackage);
            
        }

        [Test]
        public async Task DefaultActionAsync() {
            await InUI(() => _packageManagerViewModel.SwitchToAvailablePackagesAsync());
            await InUI(() => _packageManagerViewModel.SelectPackage(_packageManagerViewModel.Items.OfType<IRPackageViewModel>().SingleOrDefault(p => p.Name == TestPackages.RtvsLib1.Package)));

            _packageManagerViewModel.SelectedPackage.Should().NotBeNull();
            _packageManagerViewModel.SelectedPackage.IsInstalled.Should().BeFalse();
            _packageManagerViewModel.SelectedPackage.IsLoaded.Should().BeFalse();

            await InUI(() => _packageManagerViewModel.DefaultActionAsync());

            _packageManagerViewModel.SelectedPackage.Should().NotBeNull();
            _packageManagerViewModel.SelectedPackage.IsInstalled.Should().BeTrue();
            _packageManagerViewModel.SelectedPackage.IsLoaded.Should().BeFalse();

            await InUI(() => _packageManagerViewModel.DefaultActionAsync());

            _packageManagerViewModel.SelectedPackage.Should().NotBeNull();
            _packageManagerViewModel.SelectedPackage.IsInstalled.Should().BeTrue();
            _packageManagerViewModel.SelectedPackage.IsLoaded.Should().BeTrue();
        }
    }
}