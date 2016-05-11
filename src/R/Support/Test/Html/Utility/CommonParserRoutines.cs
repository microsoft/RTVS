// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Test.Utility;
using Microsoft.Html.Core.Tree;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public static class CommonParserRoutines {
        private static bool _regenerateBaselineFiles = false;

        static public void ParseText(HtmlTestFilesFixture fixture, string name) {
            string testFile = fixture.GetDestinationPath(name);
            string baselineFile = testFile + ".log";
            string text = fixture.LoadDestinationFile(name);
            EventLogger logger = null;

            HtmlParser p = new HtmlParser();
            logger = new EventLogger(p);
            p.Parse(text);
            var actual = logger.ToString();

            if (_regenerateBaselineFiles) {
                // Update this to your actual enlistment if you need to update baseline
                baselineFile = Path.Combine(fixture.SourcePath, name) + ".log";
                TestFiles.UpdateBaseline(baselineFile, actual);
            } else {
                TestFiles.CompareToBaseLine(baselineFile, actual);
            }
        }

        static public void BuildTree(HtmlTestFilesFixture fixture, string name) {
            string testFile = fixture.GetDestinationPath(name);
            string baselineFile = testFile + ".tree";
            string text = fixture.LoadDestinationFile(name);

            var tree = new HtmlTree(new TextStream(text), null, null, ParsingMode.Html);
            tree.Build();

            TreeWriter tw = new TreeWriter();
            string actual = tw.WriteTree(tree);

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
