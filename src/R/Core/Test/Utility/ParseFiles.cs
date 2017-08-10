// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
            var testFile = fixture.GetDestinationPath(name);
            var baselineFile = testFile + ".tree";
            var text = fixture.LoadDestinationFile(name);

            var actualTree = RParser.Parse(text);

            var astWriter = new AstWriter();
            var actual = astWriter.WriteTree(actualTree);

            if (_regenerateBaselineFiles) {
                // Update this to your actual enlistment if you need to update baseline
                baselineFile = Path.Combine(fixture.SourcePath, name) + ".tree";
                TestFiles.UpdateBaseline(baselineFile, actual);
            } else {
                TestFiles.CompareToBaseLine(baselineFile, actual);
            }
        }
    }
}
