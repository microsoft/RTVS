using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Interactive.Test.Utility {
    [ExcludeFromCodeCoverage]
    internal class ViewTreeDump {
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public static void CompareVisualTrees(TestContext context, string actual, string fileName) {
            try {
                string testFileName = fileName + ".tree";
                string testFilePath = TestFiles.GetTestFilePath(context, testFileName) ;

                if (_regenerateBaselineFiles) {
                    // Update this to your actual enlistment if you need to update baseline
                    string enlistmentPath = @"C:\RTVS\src\Package\TestApp\Files";
                    string baselineFilePath = Path.Combine(enlistmentPath, Path.GetFileName(testFileName));
                    TestFiles.UpdateBaseline(baselineFilePath, actual);
                } else {
                    TestFiles.CompareToBaseLine(testFilePath, actual);
                }
            } catch (Exception exception) {
                Assert.Fail(string.Format(CultureInfo.InvariantCulture, "Test {0} has thrown an exception: {1}", Path.GetFileName(fileName), exception.Message));
            }
        }
    }
}
