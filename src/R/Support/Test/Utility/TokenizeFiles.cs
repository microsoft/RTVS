using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Microsoft.Common.Core.Tests.Utility;
using Microsoft.Languages.Core.Tests.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Support.Test.Utility
{
    [ExcludeFromCodeCoverage]
    public static class TokenizeFiles
    {
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public static void TokenizeFile<Token, TokenType, Tokenizer>(TestContext context, string name, string language) 
            where Tokenizer: ITokenizer<Token>, new()
            where Token: IToken<TokenType>
        {
            try
            {
                string testFile = TestFiles.GetTestFilePath(context, name);
                string baselineFile = testFile + ".tokens";

                string text = TestFiles.LoadFile(context, testFile);
                ITextProvider textProvider = new TextStream(text);
                var tokenizer = new Tokenizer();

                var tokens = tokenizer.Tokenize(textProvider, 0, textProvider.Length);
                string actual = DebugWriter.WriteTokens<Token, TokenType>(tokens);

                if (_regenerateBaselineFiles)
                {
                    // Update this to your actual enlistment if you need to update baseline
                    string enlistmentPath = @"F:\RTVS\src\R\Support\Test\" + language + @"\Files\Tokenization";
                    baselineFile = Path.Combine(enlistmentPath, Path.GetFileName(testFile)) + ".tokens";

                    TestFiles.UpdateBaseline(baselineFile, actual);
                }
                else
                {
                    TestFiles.CompareToBaseLine(baselineFile, actual);
                }
            }
            catch (Exception exception)
            {
                Assert.Fail(string.Format(CultureInfo.InvariantCulture, "Test {0} has thrown an exception: {1}", Path.GetFileName(name), exception.Message));
            }
        }
    }
}
