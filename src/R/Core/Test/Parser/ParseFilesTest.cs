// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    [Category.R.Parser]
    public class ParseFilesTest {
        private readonly CoreTestFilesFixture _files;

        public ParseFilesTest(CoreTestFilesFixture files) {
            _files = files;
        }

        [CompositeTest]
        [InlineData("Check.r")]
        [InlineData("frametools.r")]
        public void ParseFile(string fileName) {
            ParseFiles.ParseFile(_files, $@"Parser\{fileName}");
        }
    }
}
