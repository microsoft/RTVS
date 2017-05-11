// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Languages.Core.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class TokenizeFiles {
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public static void TokenizeFile<TToken, TTokenType, TTokenizer>(DeployFilesFixture fixture, string name, string language)
            where TTokenizer : ITokenizer<TToken>, new()
            where TToken : IToken<TTokenType> {
            Action a = () => TokenizeFileImplementation<TToken, TTokenType, TTokenizer>(fixture, name, language);
            a.ShouldNotThrow();
        }

        private static void TokenizeFileImplementation<TToken, TTokenType, TTokenizer>(DeployFilesFixture fixture, string name, string language)
            where TTokenizer : ITokenizer<TToken>, new() where TToken : IToken<TTokenType> {
            var testFile = fixture.GetDestinationPath(name);
            var baselineFile = testFile + ".tokens";

            var text = fixture.LoadDestinationFile(name);
            var textProvider = new TextStream(text);
            var tokenizer = new TTokenizer();

            var tokens = tokenizer.Tokenize(textProvider, 0, textProvider.Length);
            var actual = DebugWriter.WriteTokens<TToken, TTokenType>(tokens);

            if (_regenerateBaselineFiles) {
                // Update this to your actual enlistment if you need to update baseline
                baselineFile = Path.Combine(fixture.SourcePath, @"Tokenization\", Path.GetFileName(testFile)) + ".tokens";
                TestFiles.UpdateBaseline(baselineFile, actual);
            } else {
                TestFiles.CompareToBaseLine(baselineFile, actual);
            }
        }
    }
}
