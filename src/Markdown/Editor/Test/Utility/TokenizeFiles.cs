using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.Markdown.Editor.Test.Utility
{
    [ExcludeFromCodeCoverage]
    public static class TokenizeFiles
    {
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public static void TokenizeFile<TToken, TTokenType, TTokenizer>(MarkdownTestFilesFixture fixture, string name, string language) 
            where TTokenizer: ITokenizer<TToken>, new()
            where TToken: IToken<TTokenType> {
            Action a = () => TokenizeFileImplementation<TToken, TTokenType, TTokenizer>(fixture, name);
            a.ShouldNotThrow();
        }

        private static void TokenizeFileImplementation<TToken, TTokenType, TTokenizer>(MarkdownTestFilesFixture fixture, string name)
            where TTokenizer : ITokenizer<TToken>, new() where TToken : IToken<TTokenType> {
            string testFile = Path.Combine(fixture.DestinationPath, name);
            string baselineFile = testFile + ".tokens";
            string text;
            using (var sr = new StreamReader(testFile)) {
                text = sr.ReadToEnd();
            }

            ITextProvider textProvider = new TextStream(text);
            var tokenizer = new TTokenizer();

            var tokens = tokenizer.Tokenize(textProvider, 0, textProvider.Length);
            string actual = DebugWriter.WriteTokens<TToken, TTokenType>(tokens);

            if (_regenerateBaselineFiles) {
                // Update this to your actual enlistment if you need to update baseline
                string enlistmentPath = @"C:\RTVS\src\Markdown\Editor\Test\Files\Tokenization";
                baselineFile = Path.Combine(enlistmentPath, Path.GetFileName(testFile)) + ".tokens";

                TestFiles.UpdateBaseline(baselineFile, actual);
            } else {
                TestFiles.CompareToBaseLine(baselineFile, actual);
            }
        }
    }
}
