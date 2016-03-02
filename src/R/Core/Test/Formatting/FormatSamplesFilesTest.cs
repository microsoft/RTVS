// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Formatting;
using Microsoft.R.Core.Formatting;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Formatting {
    [ExcludeFromCodeCoverage]
    public class FormatSamplesFilesTest {
        private readonly CoreTestFilesFixture _files;

        public FormatSamplesFilesTest(CoreTestFilesFixture files) {
            _files = files;
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFile_LeastSquares() {
            RFormatOptions options = new RFormatOptions {
                IndentType = IndentType.Tabs
            };

            FormatFilesFiles.FormatFile(_files, @"Formatting\lsfit.r", options);
        }

        [Test]
        [Category.R.Formatting]
        public void Formatter_FormatFile_IfElse() {
            RFormatOptions options = new RFormatOptions {
                IndentSize = 2
            };

            FormatFilesFiles.FormatFile(_files, @"Formatting\ifelse.r", options);
        }
    }
}
