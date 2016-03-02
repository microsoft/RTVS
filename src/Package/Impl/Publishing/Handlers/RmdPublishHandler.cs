// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Globalization;
using Microsoft.Markdown.Editor.Flavor;
using Microsoft.VisualStudio.R.Package.Publishing.Definitions;

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

        public string GetCommandLine(string inputFile, string outputFile, PublishFormat publishFormat) {
            // Run rmarkdown::render
            return string.Format(CultureInfo.InvariantCulture, "\"rmarkdown::render(\'{0}\', \'{1}\')\"", inputFile, GetDocumentTypeString(publishFormat));
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
