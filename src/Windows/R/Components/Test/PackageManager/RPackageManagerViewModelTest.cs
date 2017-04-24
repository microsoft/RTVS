using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.PackageManager.ViewModel;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit;
using static Microsoft.UnitTests.Core.Threading.UIThreadTools;

namespace Microsoft.R.Components.Test.PackageManager {
    [ExcludeFromCodeCoverage]
    [Category.PackageManager]
    public class RPackageManagerViewModelTest : IAsyncLifetime {
        private readonly IServiceContainer _services;
        private readonly TestFilesFixture _testFiles;
        private readonly MethodInfo _testMethod;
        private readonly IRInteractiveWorkflow _workflow;
        private IRPackageManagerVisualComponent _packageManagerComponent;
        private IRPackageManagerViewModel _packageManagerViewModel;

        public RPackageManagerViewModelTest(IServiceContainer services, TestMethodFixture testMethod, TestFilesFixture testFiles) {
            _services = services;
            var workflowProvider = _services.GetService<IRInteractiveWorkflowProvider>();
            _workflow = UIThreadHelper.Instance.Invoke(() => workflowProvider.GetOrCreate());
            _testMethod = testMethod.MethodInfo;
            _testFiles = testFiles;
        }

        public async Task InitializeAsync() {
            var settings = _services.GetService<IRSettings>();
            await _workflow.RSessions.TrySwitchBrokerAsync(nameof(RPackageManagerViewModelTest));

            await _workflow.RSession.EnsureHostStartedAsync(new RHostStartupInfo(settings.CranMirror, codePage: settings.RCodePage), null, 50000);

            await TestRepositories.SetLocalRepoAsync(_workflow.RSession, _testFiles);
            await TestLibraries.SetLocalLibraryAsync(_workflow.RSession, _testMethod, _testFiles);

            var componentContainerFactory = _services.GetService<IRPackageManagerVisualComponentContainerFactory>();
            _packageManagerComponent = await InUI(() => _workflow.Packages.GetOrCreateVisualComponent(componentContainerFactory));
            _packageManagerViewModel = await InUI(() => _packageManagerComponent.Control.DataContext) as IRPackageManagerViewModel;
        }

        public Task DisposeAsync() {
            _packageManagerComponent.Dispose();
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

            await ParallelTools.WhenAll(t1, t2, t3);

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

            await ParallelTools.WhenAll(t1, t2, t3);
            var expected = new [] { "NotAvailable1", "NotAvailable2", "rtvslib1" };

            _packageManagerViewModel.IsLoading.Should().BeFalse();
            _packageManagerViewModel.SelectedPackage.Should().NotBeNull();
            _packageManagerViewModel.Items.Should().Equal(expected, (o, n) => ((IRPackageViewModel)o).Name.EqualsOrdinal(n))
                .And.Contain(_packageManagerViewModel.SelectedPackage);
            
        }

        [Test]
        public async Task DefaultActionAsync() {
            var package = TestPackages.CreateRtvsLib1().Package;
            await InUI(() => _packageManagerViewModel.SwitchToAvailablePackagesAsync());
            await InUI(() => _packageManagerViewModel.SelectPackage(_packageManagerViewModel.Items.OfType<IRPackageViewModel>().SingleOrDefault(p => p.Name == package)));

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

        [Test(ThreadType.UI)]
        public async Task SwitchFromInstalledToAvailableWhileLoadingInstalled() {
            // We need real repo for this test
            await TestRepositories.SetCranRepoAsync(_workflow.RSession);

            var t0 = _packageManagerViewModel.SwitchToLoadedPackagesAsync();
            var t1 = _packageManagerViewModel.SwitchToInstalledPackagesAsync();
            var t2 = _packageManagerViewModel.SwitchToAvailablePackagesAsync();

            await t1;
            _packageManagerViewModel.IsLoading.Should().BeTrue();

            await t2;
            _packageManagerViewModel.IsLoading.Should().BeFalse();

            await t0;
        }
    }
}