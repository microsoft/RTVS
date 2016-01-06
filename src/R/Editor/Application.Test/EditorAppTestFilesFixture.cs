using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.Application.Test {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class EditorAppTestFilesFixture : DeployFilesFixture {
        public EditorAppTestFilesFixture() : base(@"R\Editor\Application.Test\Files", "Files") { }
    }
}
