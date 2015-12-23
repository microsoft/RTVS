using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Test {
    public class EditorTestFilesFixture : DeployFilesFixture {
        public EditorTestFilesFixture() : base(@"R\Editor\Test\Files", "Files") {
        }
    }

    [CollectionDefinition(nameof(EditorTestFilesCollection))]
    public class EditorTestFilesCollection : ICollectionFixture<EditorTestFilesFixture> {
    }
}