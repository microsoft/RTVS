// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Markdown.Editor.Test.Utility;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Markdown.Editor.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class TokenizeSampleMdFilesTest {
        private readonly MarkdownTestFilesFixture _files;

        public TokenizeSampleMdFilesTest(MarkdownTestFilesFixture files) {
            _files = files;
        }

        [Test]
        [Category.Md.Tokenizer]
        public void TokenizeSampleMdFile01() {
            TokenizeFiles.TokenizeFile<MarkdownToken, MarkdownTokenType, MdTokenizer>(_files, @"Tokenization\01.md", "Markdown");
        }
    }
}
