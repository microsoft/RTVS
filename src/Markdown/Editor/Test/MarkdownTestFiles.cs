using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Markdown.Editor.Test {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class MarkdownTestFilesFixture : DeployFilesFixture {
        public MarkdownTestFilesFixture() : base(@"Markdown\Editor\Test\Files", "Files") { }
    }
}
