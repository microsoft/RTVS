using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Tokens;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.R.Support.Markdown.Tokens;
using Microsoft.R.Support.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Support.Test.Markdown.Tokens
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TokenizeSampleMdFilesTest : TokenizeTestBase<MdToken, MdTokenType>
    {
        [TestMethod]
        public void TokenizeSampleMdFile01()
        {
            EditorShell.SetShell(TestEditorShell.Create());
            TokenizeFiles.TokenizeFile<MdToken, MdTokenType, MdTokenizer>(this.TestContext, @"Tokenization\01.md", "Markdown");
        }
    }
}
