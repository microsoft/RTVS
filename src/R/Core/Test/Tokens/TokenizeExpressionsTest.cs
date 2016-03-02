// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class TokenizeExpressionsTest {
        private readonly CoreTestFilesFixture _files;

        public TokenizeExpressionsTest(CoreTestFilesFixture files) {
            _files = files;
        }

        [Test]
        [Category.R.Tokenizer]
        public void TokenizeFile_ExpressionsFile() {
            TokenizeFiles.TokenizeFile(_files, @"Tokenization\Expressions.r");
        }
    }
}
