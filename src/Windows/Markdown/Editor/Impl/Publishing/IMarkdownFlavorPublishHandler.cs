// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.Markdown.Editor.Flavor;
using Microsoft.R.Host.Client;

namespace Microsoft.Markdown.Editor.Publishing {
    public interface IMarkdownFlavorPublishHandler {
        MarkdownFlavor Flavor { get; }
        string RequiredPackageName { get; }
        bool FormatSupported(PublishFormat format);
        Task PublishAsync(IRSession session, IServiceContainer services, string inputFilePath, string outputFilePath, PublishFormat publishFormat, Encoding encoding);
    }
}
