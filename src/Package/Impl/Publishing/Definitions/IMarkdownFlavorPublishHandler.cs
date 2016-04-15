// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text;
using Microsoft.Markdown.Editor.Flavor;

namespace Microsoft.VisualStudio.R.Package.Publishing.Definitions {
    public interface IMarkdownFlavorPublishHandler {
        MarkdownFlavor Flavor { get; }
        string RequiredPackageName { get; }
        bool FormatSupported(PublishFormat format);
        string GetCommandLine(string inputFile, string outputFile, PublishFormat publishFormat, Encoding encoding);
    }
}
