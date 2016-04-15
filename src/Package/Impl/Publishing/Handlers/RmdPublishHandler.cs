// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.IO;
using Microsoft.Markdown.Editor.Flavor;
using Microsoft.VisualStudio.R.Package.Publishing.Definitions;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Publishing {

    [Export(typeof(IMarkdownFlavorPublishHandler))]
    internal sealed class RmdPublishHandler : IMarkdownFlavorPublishHandler {

        public MarkdownFlavor Flavor {
            get { return MarkdownFlavor.R; }
        }

        public string RequiredPackageName {
            get { return "rmarkdown"; }
        }

        public bool FormatSupported(PublishFormat format) {
            return format != PublishFormat.Pdf;
        }

        public string GetCommandLine(string inputFile, string outputFilePath, PublishFormat publishFormat) {
            string format = GetDocumentTypeString(publishFormat);
            string outputFile = Path.GetFileName(outputFilePath);
            string outputFolder = Path.GetDirectoryName(outputFilePath).Replace('\\', '/');
            // Run rmarkdown::render
            return Invariant($"\"rmarkdown::render('{inputFile}', output_format='{format}', output_file='{outputFile}',  output_dir='{outputFolder}', encoding='UTF-8')\"");
        }

        private string GetDocumentTypeString(PublishFormat publishFormat) {
            switch (publishFormat) {
                case PublishFormat.Pdf:
                    return "pdf_document";

                case PublishFormat.Word:
                    return "word_document";
            }

            return "html_document";
        }
    }
}
