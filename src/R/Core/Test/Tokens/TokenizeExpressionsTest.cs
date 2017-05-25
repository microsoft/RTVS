// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.Tokens;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    [Category.R.Tokenizer]
    public class TokenizeExpressionsTest {
        private readonly CoreTestFilesFixture _files;

        public TokenizeExpressionsTest(CoreTestFilesFixture files) {
            _files = files;
        }

        [Test]
        public void TokenizeFile_ExpressionsFile()
            => TokenizeFiles.TokenizeFile<RToken, RTokenType, RTokenizer>(_files, @"Tokenization\Expressions.r", "R");
    }
}
