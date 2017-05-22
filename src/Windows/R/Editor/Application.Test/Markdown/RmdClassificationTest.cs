// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Editor.Test.Utility;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Markdown {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class RmdClassificationTest {
        private static bool _regenerateBaselineFiles = false;

        private readonly IServiceContainer _services;
        private readonly EditorHostMethodFixture _editorHost;
        private readonly EditorAppTestFilesFixture _files;

        public RmdClassificationTest(IServiceContainer services, EditorHostMethodFixture editorHost, EditorAppTestFilesFixture files) {
            _services = services;
            _editorHost = editorHost;
            _files = files;
        }

        [CompositeTest]
        [Category.Interactive]
        [InlineData("01.rmd")]
        public async Task RColors(string fileName) {
            string content = _files.LoadDestinationFile(fileName);
            using (var script = await _editorHost.StartScript(_services, content, fileName, MdContentTypeDefinition.ContentType, null)) {
                script.DoIdle(500);
                var spans = script.GetClassificationSpans();
                var actual = ClassificationWriter.WriteClassifications(spans);
                VerifyClassifications(fileName, actual);
            }
        }

        public void VerifyClassifications(string testFileName, string actual) {
            var testFilePath = _files.GetDestinationPath(testFileName);
            string baselineFile = testFilePath + ".colors";

            if (_regenerateBaselineFiles) {
                baselineFile = Path.Combine(_files.SourcePath, testFileName) + ".colors";
                TestFiles.UpdateBaseline(baselineFile, actual);
            } else {
                TestFiles.CompareToBaseLine(baselineFile, actual);
            }
        }
    }
}
