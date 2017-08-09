// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    [Category.R.Parser]
    public class ParseFilesTest {
        private readonly CoreTestFilesFixture _files;

        public ParseFilesTest(CoreTestFilesFixture files) {
            _files = files;
        }

        [Test]
        public void ParseFile_CheckR() {
            ParseFiles.ParseFile(_files, @"Parser\Check.r");
        }

        [Test]
        public void ParseFile_FrametoolsR() {
            ParseFiles.ParseFile(_files, @"Parser\frametools.r");
        }
    }
}
