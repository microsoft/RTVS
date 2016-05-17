// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Formatting;
using Microsoft.R.Core.Formatting;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.R.Formatting]
    public class FormatSamplesFilesTest {
        private readonly CoreTestFilesFixture _files;

        public FormatSamplesFilesTest(CoreTestFilesFixture files) {
            _files = files;
        }

        [Test]
        public void LeastSquares() {
            RFormatOptions options = new RFormatOptions {
                IndentType = IndentType.Tabs
            };

            FormatFilesFiles.FormatFile(_files, @"Formatting\lsfit.r", options);
        }

        [Test]
        public void IfElse() {
            RFormatOptions options = new RFormatOptions {
                IndentSize = 2
            };

            FormatFilesFiles.FormatFile(_files, @"Formatting\ifelse.r", options);
        }

        [Test]
        public void Args() {
            RFormatOptions options = new RFormatOptions {
                IndentSize = 4
            };

            FormatFilesFiles.FormatFile(_files, @"Formatting\args.r", options);
        }
    }
}
