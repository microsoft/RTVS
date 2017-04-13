// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.RData.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class TokenizeSampleRdFilesTest {
        private readonly SupportTestFilesSetup _files;

        public TokenizeSampleRdFilesTest(SupportTestFilesSetup files) {
            _files = files;
        }

        [CompositeTest]
        [InlineData(@"Tokenization\01.rd")]
        [InlineData(@"Tokenization\02.rd")]
        [InlineData(@"Tokenization\03.rd")]
        [InlineData(@"Tokenization\04.rd")]
        [InlineData(@"Tokenization\05.rd")]
        [InlineData(@"Tokenization\06.rd")]
        [InlineData(@"Tokenization\07.rd")]
        [InlineData(@"Tokenization\08.rd")]
        [InlineData(@"Tokenization\09.rd")]
        [InlineData(@"Tokenization\10.rd")]
        [InlineData(@"Tokenization\11.rd")]
        [InlineData(@"Tokenization\12.rd")]
        [Category.Rd.Tokenizer]
        public void TokenizeSampleRdFile(string fileName) {
            TokenizeFiles.TokenizeFile<RdToken, RdTokenType, RdTokenizer>(_files, fileName, "RD");
        }
    }
}
