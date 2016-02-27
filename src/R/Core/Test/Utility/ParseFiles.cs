using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Utility;

namespace Microsoft.R.Core.Test.Utility {
    [ExcludeFromCodeCoverage]
    public class ParseFiles
    {
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public static void ParseFile(CoreTestFilesFixture fixture, string name) {
            Action a = () => ParseFileImplementation(fixture, name);
            a.ShouldNotThrow();
        }

        private static void ParseFileImplementation(CoreTestFilesFixture fixture, string name) {
            string testFile = fixture.GetDestinationPath(name);
            string baselineFile = testFile + ".tree";
            string text = fixture.LoadDestinationFile(name);

            AstRoot actualTree = RParser.Parse(text);

            AstWriter astWriter = new AstWriter();
            string actual = astWriter.WriteTree(actualTree);

            if (_regenerateBaselineFiles) {
                // Update this to your actual enlistment if you need to update baseline
                string enlistmentPath = @"C:\RTVS-Main\src\R\Core\Test\Files\Parser";
                baselineFile = Path.Combine(enlistmentPath, Path.GetFileName(testFile)) + ".tree";

                TestFiles.UpdateBaseline(baselineFile, actual);
            } else {
                TestFiles.CompareToBaseLine(baselineFile, actual);
            }
        }
    }
}
