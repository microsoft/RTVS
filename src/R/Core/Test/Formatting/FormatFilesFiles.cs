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

        public static void FormatFile(CoreTestFilesFixture fixture, string name) {
            FormatFile(fixture, name, new RFormatOptions());
        }

        public static void FormatFile(CoreTestFilesFixture fixture, string name, RFormatOptions options) {
            Action a = () => FormatFileImplementation(fixture, name, options);
            a.ShouldNotThrow();
        }

        private static void FormatFileImplementation(CoreTestFilesFixture fixture, string name, RFormatOptions options) {
            string testFile = Path.Combine(fixture.DestinationPath, name);
            string baselineFile = testFile + ".formatted";

            string text;
            using (var sr = new StreamReader(testFile)) {
                text = sr.ReadToEnd();
            }

            RFormatter formatter = new RFormatter(options);

            string actual = formatter.Format(text);
            if (_regenerateBaselineFiles) {
                // Update this to your actual enlistment if you need to update baseline
                string enlistmentPath = @"C:\RTVS\src\R\Core\Test\Files\Formatting";
                baselineFile = Path.Combine(enlistmentPath, Path.GetFileName(testFile)) + ".formatted";

                TestFiles.UpdateBaseline(baselineFile, actual);
            } else {
                TestFiles.CompareToBaseLine(baselineFile, actual);
            }
        }
    }
}
