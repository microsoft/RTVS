// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Parser {
    [ExcludeFromCodeCoverage]
    public class HtmlParserFilesTest {
        private readonly HtmlTestFilesFixture _files;

        public HtmlParserFilesTest(HtmlTestFilesFixture files) {
            _files = files;
        }

        [CompositeTest]
        [Category.Html]
        [InlineData("broken_01.htm")]
        [InlineData("broken_02.htm")]
        [InlineData("broken_03.htm")]
        [InlineData("cnn.htm")]
        [InlineData("doctype.htm")]
        [InlineData("implicit.htm")]
        [InlineData("meta-charset.htm")]
        [InlineData("namespaces.htm")]
        [InlineData("xhtml.htm")]
        public void HtmlParseFiles(string fileName) {
            CommonParserRoutines.BuildTree(_files, fileName);
        }

        [CompositeTest]
        [Category.Html]
        [InlineData("broken_01.htm")]
        [InlineData("broken_02.htm")]
        [InlineData("broken_03.htm")]
        [InlineData("cnn.htm")]
        [InlineData("doctype.htm")]
        [InlineData("implicit.htm")]
        [InlineData("meta-charset.htm")]
        [InlineData("namespaces.htm")]
        [InlineData("xhtml.htm")]
        public void HtmlLogFiles(string fileName) {
            CommonParserRoutines.ParseText(_files, fileName);
        }
    }
}
