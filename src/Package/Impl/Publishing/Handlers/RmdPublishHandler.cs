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
                await RMarkdownRenderAsync(session, fs, inputFilePath, outputFilePath, GetDocumentTypeString(publishFormat), encoding.CodePage);
            } catch (IOException ex) {
                coreShell.ShowErrorMessage(ex.Message);
            } catch (RException ex) {
                coreShell.ShowErrorMessage(ex.Message);
            } catch (OperationCanceledException) {
            } 
        }

        private async Task RMarkdownRenderAsync(IRSession session, IFileSystem fs, string inputFilePath, string outputFilePath, string format, int codePage) {
            using (var fts = new DataTransferSession(session, fs)) {
                var rmd = await fts.SendFileAsync(inputFilePath);
                var publishResult = await session.EvaluateAsync<byte[]>($"rtvs:::rmarkdown_publish(blob_id = {rmd.Id}, output_format = {format.ToRStringLiteral()}, encoding = 'cp{codePage}')", REvaluationKind.Normal);
                File.WriteAllBytes(outputFilePath, publishResult);
            }
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
