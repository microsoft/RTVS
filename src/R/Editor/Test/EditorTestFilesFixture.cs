using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.Test {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class EditorTestFilesFixture : DeployFilesFixture {
        public EditorTestFilesFixture() : base(@"R\Editor\Test\Files", "Files") {}
    }
}