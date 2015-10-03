using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.Markdown.Editor.Test.Utility;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.R.Support.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Markdown.Editor.Test.Tokens
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeSampleMdFilesTest : TokenizeTestBase<MarkdownToken, MarkdownTokenType>
    {
        [TestMethod]
        public void TokenizeSampleMdFile01()
        {
            EditorShell.SetShell(TestEditorShell.Create(MarkdownTestCompositionCatalog.Current));
            TokenizeFiles.TokenizeFile<MarkdownToken, MarkdownTokenType, MdTokenizer>(this.TestContext, @"Tokenization\01.md", "Markdown");
        }
    }
}
