// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Markdown.Editor.Flavor;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
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
            return true;
        }

        public async Task PublishAsync(IRSession session, ICoreShell coreShell,  IFileSystem fs, string inputFilePath, string outputFilePath, PublishFormat publishFormat, Encoding encoding) {
            try {
                using(var fts = new FileTransferSession(session, fs)) {
                    var rmd = await fts.SendFileAsync(inputFilePath);
                    var result = await RMarkdownRenderAsync(session, rmd, outputFilePath, publishFormat, encoding);
                    await fts.FetchFileAsync(result, outputFilePath);
                }
            } catch (IOException ex) {
                coreShell.ShowErrorMessage(ex.Message);
            } catch (RException ex) {
                coreShell.ShowErrorMessage(ex.Message);
            } catch (OperationCanceledException) {
            } 
        }

        private async Task<IRBlobFileInfo> RMarkdownRenderAsync(IRSession session, IRBlob blob, string outputFilePath, PublishFormat publishFormat, Encoding encoding) {
            string outputFile = Path.GetFileName(outputFilePath).ToRStringLiteral();
            string format = GetDocumentTypeString(publishFormat).ToRStringLiteral();
            var publishResult = await session.EvaluateAsync($"rtvs:::rmarkdown_publish(blob_id = {blob.Id}, output_filename = {outputFile}, output_format = {format}, encoding = 'cp{encoding.CodePage}')", REvaluationKind.Normal);
            return BlobFileInfo.Create(publishResult.Result);
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
