// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Core.Formatting;

namespace Microsoft.R.Core.Test.Formatting {
    [ExcludeFromCodeCoverage]
    public class FormatFilesFiles {
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public static void FormatFile(CoreTestFilesFixture fixture, string name, RFormatOptions options) {
            Action a = () => FormatFileImplementation(fixture, name, options);
            a.ShouldNotThrow();
        }

        private static void FormatFileImplementation(CoreTestFilesFixture fixture, string name, RFormatOptions options) {
            string testFile = fixture.GetDestinationPath(name);
            string baselineFile = testFile + ".formatted";
            string text = fixture.LoadDestinationFile(name);

            RFormatter formatter = new RFormatter(options);

            string actual = formatter.Format(text);
            if (_regenerateBaselineFiles) {
                // Update this to your actual enlistment if you need to update baseline
                baselineFile = Path.Combine(fixture.SourcePath, @"Formatting\", Path.GetFileName(testFile)) + ".formatted";
                TestFiles.UpdateBaseline(baselineFile, actual);
            } else {
                TestFiles.CompareToBaseLine(baselineFile, actual);
            }
        }
    }
}
