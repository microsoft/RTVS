using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.VisualStudio.R.Interactive.Test.Utility {
    [ExcludeFromCodeCoverage]
    internal class ViewTreeDump {
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public static void CompareVisualTrees(DeployFilesFixture fixture, string actual, string fileName) {
            Action a = () => CompareVisualTreesImplementation(fixture, actual, fileName);
            a.ShouldNotThrow();
        }

        private static void CompareVisualTreesImplementation(DeployFilesFixture fixture, string actual, string fileName) {
            string testFileName = fileName + ".tree";
            string testFilePath = fixture.GetDestinationPath(testFileName);

            if (_regenerateBaselineFiles) {
                // Update this to your actual enlistment if you need to update baseline
                string enlistmentPath = @"F:\RTVS\src\Package\TestApp\Files";
                string baselineFilePath = Path.Combine(enlistmentPath, Path.GetFileName(testFileName));
                TestFiles.UpdateBaseline(baselineFilePath, actual);
            } else {
                TestFiles.CompareToBaseLine(testFilePath, actual);
            }
        }
    }
}
