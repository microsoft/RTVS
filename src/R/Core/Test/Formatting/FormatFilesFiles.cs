using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Microsoft.Common.Core.Tests.Utility;
using Microsoft.Languages.Core.Tests.Utility;
using Microsoft.R.Core.Formatting;
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
            FormatFile(context, name, new RFormatOptions());
        }

        public static void FormatFile(TestContext context, string name, RFormatOptions options)
        {
            try
            {
                string testFile = TestFiles.GetTestFilePath(context, name);
                string baselineFile = testFile + ".formatted";

                string text = TestFiles.LoadFile(context, testFile);
                RFormatter formatter = new RFormatter(options);

                string actual = formatter.Format(text);
                if (_regenerateBaselineFiles)
                {
                    // Update this to your actual enlistment if you need to update baseline
                    string enlistmentPath = @"C:\RTVS\src\R\Core\Test\Files\Formatting";
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
