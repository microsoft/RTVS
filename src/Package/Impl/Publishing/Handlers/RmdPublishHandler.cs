// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Markdown.Editor.Flavor;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.R.Package.Publishing.Definitions;
using Newtonsoft.Json.Linq;

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
            long rmdBlobId = 0;
            long outputRmdBlobId = 0;

            try {
                // Send the markdown to R-Host for processing as a blob
                rmdBlobId = await session.SendFileAsBlobAsync(fs, inputFilePath);
                if (rmdBlobId <= 0) {
                    return;
                }

                // Perform markdown rendering on the blob
                JObject jsonObj = await RMarkdownRenderAsync(session, rmdBlobId, outputFilePath, publishFormat, encoding);

                // Markdown results should be in a single blob (file) output. The response for markdown rendering should contain 
                // the blob id and the output file name.
                JArray jsonBlobids = (JArray)jsonObj["blob_ids"];
                JArray jsonFileNames = (JArray)jsonObj["file_names"];
                if (jsonBlobids.Count != 1 || jsonFileNames.Count != 1) {
                    return;
                }

                // Get the published content from R-Host
                outputRmdBlobId = jsonBlobids[0].Value<long>();
                string fullOutputFilePath = Path.Combine(Path.GetDirectoryName(outputFilePath), jsonFileNames[0].Value<string>());
                await session.GetFileFromBlobAsync(fs, outputRmdBlobId, fullOutputFilePath);

            } catch (IOException ex) {
                coreShell.ShowErrorMessage(ex.Message);
            } catch (RException) {
            } catch (OperationCanceledException) {
            } finally {
                // Clean-up blobs that are no longer needed
                await session.DestroyBlobAsync(new long[] { rmdBlobId, outputRmdBlobId });
            }
        }

        private async Task<JObject> RMarkdownRenderAsync(IRSession session, long blobId, string outputFilePath, PublishFormat publishFormat, Encoding encoding) {
            string outputFile = Path.GetFileName(outputFilePath).ToRStringLiteral();
            string format = GetDocumentTypeString(publishFormat).ToRStringLiteral();
            var publishResult = await session.EvaluateAsync($"rtvs:::rmarkdown_publish(blob_id = {blobId}, output_filename = {outputFile}, output_format = {format}, encoding = 'cp{encoding.CodePage}')", REvaluationKind.Normal);
            return  (JObject)publishResult.Result;
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
