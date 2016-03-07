// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class TokenizeFiles {
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public static void TokenizeFile(CoreTestFilesFixture fixture, string name) {
            Action a = () => TokenizeFileImplementation(fixture, name);
            a.ShouldNotThrow();
        }

        public static void TokenizeFileImplementation(CoreTestFilesFixture fixture, string name) {
            string testFile = fixture.GetDestinationPath(name);
            string baselineFile = testFile + ".tokens";
            string text = fixture.LoadDestinationFile(name);

            ITextProvider textProvider = new TextStream(text);
            var tokenizer = new RTokenizer();

            var tokens = tokenizer.Tokenize(textProvider, 0, textProvider.Length);
            string actual = DebugWriter.WriteTokens<RToken, RTokenType>(tokens);

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
