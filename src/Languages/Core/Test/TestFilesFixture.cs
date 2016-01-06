using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Languages.Core.Test {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class TestFilesFixture : DeployFilesFixture {
        public TestFilesFixture() : base(@"Common\Core\Test\Files", "Files") { }
    }
}
