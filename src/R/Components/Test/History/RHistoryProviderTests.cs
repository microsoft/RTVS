using System.ComponentModel.Composition.Hosting;
using FluentAssertions;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Test.StubBuilders;
using Microsoft.R.Components.History;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.Test.StubFactories;
using Microsoft.R.Components.Test.Stubs;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Components.Test.History {
    public class RHistoryProviderTests {
        private readonly RComponentsMefCatalogFixture _mefCatalogFixture;

        public RHistoryProviderTests(RComponentsMefCatalogFixture mefCatalogFixture) {
            _mefCatalogFixture = mefCatalogFixture;
        }

        [Test]
        public void MefComposition() {
            var additionalValues = new CompositionBatch()
                .AddValue(TextSearchServiceStubFactory.CreateDefault())
                .AddValue(RtfBuilderServiceStubFactory.CreateDefault())
                .AddValue(FileSystemStubFactory.CreateDefault())
                .AddValue<IRSettings>(new RSettingsStub());

            var exportProvider = _mefCatalogFixture.CreateExportProvider(additionalValues);
            exportProvider.GetExportedValue<IRHistoryProvider>().Should().NotBeNull();
        }
    }
}