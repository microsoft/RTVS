using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Support.Test.Utility {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class SupportTestFilesSetup : DeployFilesFixture {
        public SupportTestFilesSetup() : base(@"R\Support\Test\RD\Files", "Files") {}
    }
}
