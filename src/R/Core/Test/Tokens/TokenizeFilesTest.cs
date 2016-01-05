using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Test.Tokens
{
    [ExcludeFromCodeCoverage]
    public class TokenizeFiles
    {
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public static void TokenizeFile(CoreTestFilesFixture fixture, string name) {
            Action a = () => TokenizeFileImplementation(fixture, name);
            a.ShouldNotThrow();
        }

        public static void TokenizeFileImplementation(CoreTestFilesFixture fixture, string name)
        {
            string testFile = Path.Combine(fixture.DestinationPath, name);
            string baselineFile = testFile + ".tokens";

            string text;
            using (var sr = new StreamReader(testFile)) {
                text = sr.ReadToEnd();
            }

            ITextProvider textProvider = new TextStream(text);
            var tokenizer = new RTokenizer();

            var tokens = tokenizer.Tokenize(textProvider, 0, textProvider.Length);
            string actual = DebugWriter.WriteTokens<RToken, RTokenType>(tokens);

            if (_regenerateBaselineFiles) {
                // Update this to your actual enlistment if you need to update baseline
                string enlistmentPath = @"C:\RTVS\src\R\Core\Test\Files\Tokenization";
                baselineFile = Path.Combine(enlistmentPath, Path.GetFileName(testFile)) + ".tokens";

                TestFiles.UpdateBaseline(baselineFile, actual);
            } else {
                TestFiles.CompareToBaseLine(baselineFile, actual);
            }
        }
    }
}
