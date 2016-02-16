using System.ComponentModel.Composition.Hosting;
using FluentAssertions;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.InteractiveWindow;

namespace Microsoft.R.Components.Test.UI {
    /// <summary>
    /// These tests are basic markers that all required composition imports are available.
    /// </summary>
    public class MefCompositionTests {
        private readonly ExportProvider _exportProvider;

        public MefCompositionTests(RComponentsUIMefCatalogFixture mefCatalog) {
            _exportProvider = mefCatalog.CreateExportProvider();
        }

        [Test]
        public void RHistoryProvider() {
            _exportProvider.GetExportedValue<IRHistoryProvider>().Should().NotBeNull();
        }

        [Test]
        public void RInteractiveWorkflowProvider() {
            _exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().Should().NotBeNull();
        }

        [Test]
        public void RHistoryVisualComponentContainerFactory() {
            _exportProvider.GetExportedValue<IRHistoryVisualComponentContainerFactory>().Should().NotBeNull();
        }

        [Test]
        public void InteractiveWindowComponentContainerFactory() {
            _exportProvider.GetExportedValue<IInteractiveWindowComponentContainerFactory>().Should().NotBeNull();
        }

        [Test]
        public void InteractiveWindowFactoryService() {
            _exportProvider.GetExportedValue<IInteractiveWindowFactoryService>().Should().NotBeNull();
        }
    }
}
