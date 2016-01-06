using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class CoreTestFilesFixture : DeployFilesFixture {
        public CoreTestFilesFixture() : base(@"R\Core\Test\Files", "Files") { }
    }
}
