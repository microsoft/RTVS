using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Microsoft.Languages.Core.Diagnostics;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Core.Test.Tokens
{
    [ExcludeFromCodeCoverage]
    public class FormatFilesFiles
    {
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public static void FormatFile(TestContext context, string name)
        {
            try
            {
                string testFile = TestFiles.GetTestFilePath(context, name);
                string baselineFile = testFile + ".formatted";

                string text = TestFiles.LoadFile(context, testFile);
                RFormatter formatter = new RFormatter();

                string actual = formatter.Format(text);
                if (_regenerateBaselineFiles)
                {
                    // Update this to your actual enlistment if you need to update baseline
                    string enlistmentPath = @"F:\RTVS\src\R\Core\Test\Files\Formatting";
                    baselineFile = Path.Combine(enlistmentPath, Path.GetFileName(testFile)) + ".formatted";

                    TestFiles.UpdateBaseline(baselineFile, actual);
                }
                else
                {
                    TestFiles.CompareToBaseLine(baselineFile, actual);
                }
            }
            catch (Exception exception)
            {
                Assert.Fail(string.Format(CultureInfo.InvariantCulture, "Test {0} has thrown an exception: {1}", Path.GetFileName(name), exception.Message));
            }
        }
    }
}
