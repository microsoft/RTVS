using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.VisualStudio.R.Interactive.Test {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class TestFilesFixture : DeployFilesFixture {
        public TestFilesFixture() : base(@"Package\TestApp\Files", "Files") { }
    }
}
