// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Markdown.Editor.Flavor;
using Microsoft.R.Host.Client;
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

        public async Task Publish(IRSession session, string inputFilePath, string outputFilePath, PublishFormat publishFormat, Encoding encoding) {
            // Send the markdown to R-Host for processing as a blob
            byte[] rmdData = File.ReadAllBytes(inputFilePath);
            var rmdBlobId = await session.SendBlobAsync(rmdData);

            // Perform markdown rendering on the blob
            string outputFile = Path.GetFileName(outputFilePath);
            string format = GetDocumentTypeString(publishFormat);
            string ext = Path.GetExtension(outputFilePath).Substring(1);
            var publishResult = await session.EvaluateAsync($"rtvs:::rmarkdown_publish(blob_id = {rmdBlobId}, output_filename = '{outputFile}', output_format='{format}', encoding='cp{encoding.CodePage}')", REvaluationKind.Normal);
            JObject obj = publishResult.Result as JObject;
            JArray jsonBlobids = obj["blob.ids"] as JArray;
            JArray jsonFileNames = obj["file.names"] as JArray;

            // The response should contain the blob id and the output file name
            long[] blobids = new long[jsonBlobids.Count];
            Dictionary<long, string> fileNameMap = new Dictionary<long, string>();
            int i = 0;
            foreach (var jblob in jsonBlobids) {
                blobids[i] = jblob.Value<long>();
                fileNameMap.Add(blobids[i], jsonFileNames[i].Value<string>());
                ++i;
            }

            // Get the published content from R-Host
            var data = await session.GetBlobAsync(blobids);

            // Save it locally 
            string outputFolder = Path.GetDirectoryName(outputFilePath);
            foreach (Blob blob in data) {
                string fullPath = Path.Combine(outputFolder, fileNameMap[blob.BlobId]);
                File.WriteAllBytes(fullPath, blob.Data);
            }

            // Clean up all the blobs that were created during publishing
            long[] blobids2 = new long[blobids.Length + 1];
            blobids2[0] = rmdBlobId;
            Array.ConstrainedCopy(blobids, 0, blobids2, 1, blobids.Length);
            await session.DestroyBlobAsync(blobids);
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
