using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.Common.Core.Tests.Utility;
using Microsoft.Languages.Core.Tests.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Xunit;

namespace Microsoft.Languages.Editor.Tests.Utility {
    [ExcludeFromCodeCoverage]
    public static class TokenizeFiles
    {
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public static void TokenizeFile<Token, TokenType>(string testFile, string language, Type tokenizerType) 
            where Token: IToken<TokenType>
        {
            try
            {
                string baselineFile = testFile + ".tokens";
                using (var sr = new StreamReader(testFile)) {
                    string text = sr.ReadToEnd();
                    ITextProvider textProvider = new TextStream(text);
                    var tokenizer = Activator.CreateInstance(tokenizerType) as ITokenizer<Token>;

                    var tokens = tokenizer.Tokenize(textProvider, 0, textProvider.Length);
                    string actual = DebugWriter.WriteTokens<Token, TokenType>(tokens);

                    if (_regenerateBaselineFiles) {
                        // Update this to your actual enlistment if you need to update baseline
                        string enlistmentPath = @"F:\RTVS\src\R\Support\Test\" + language + @"\Files\Tokenization";
                        baselineFile = Path.Combine(enlistmentPath, Path.GetFileName(testFile)) + ".tokens";

                        TestFiles.UpdateBaseline(baselineFile, actual);
                    } else {
                        TestFiles.CompareToBaseLine(baselineFile, actual);
                    }
                }
            }
            catch (Exception exception)
            {
                Assert.False(false, string.Format(CultureInfo.InvariantCulture, "Test {0} has thrown an exception: {1}", 
                              Path.GetFileName(testFile), exception.Message));
            }
        }
    }
}
