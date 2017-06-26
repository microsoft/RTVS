// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Markdown.Editor.Flavor;
using Microsoft.R.Components.StatusBar;
using Microsoft.R.Host.Client;

namespace Microsoft.Markdown.Editor.Publishing {

    [Export(typeof(IMarkdownFlavorPublishHandler))]
    internal sealed class RmdPublishHandler : IMarkdownFlavorPublishHandler {
        public MarkdownFlavor Flavor => MarkdownFlavor.R;
        public string RequiredPackageName => "rmarkdown";
        public bool FormatSupported(PublishFormat format) => true;

        public async Task PublishAsync(IRSession session, IServiceContainer services, string inputFilePath, string outputFilePath, PublishFormat publishFormat, Encoding encoding) {
            try {
                await RMarkdownRenderAsync(session, inputFilePath, outputFilePath, GetDocumentTypeString(publishFormat), encoding.CodePage, services);
            } catch (IOException ex) {
                await services.ShowErrorMessageAsync(ex.Message);
            } catch (RException ex) {
                await services.ShowErrorMessageAsync(ex.Message);
            } catch (OperationCanceledException) {
            }
        }

        private async Task RMarkdownRenderAsync(IRSession session, string inputFilePath, string outputFilePath, string format, int codePage, IServiceContainer services) {
            using (var fts = new DataTransferSession(session, services.FileSystem())) {
                var statusBar = services.GetService<IStatusBar>();
                using (var progress = await statusBar.ShowProgressAsync(3)) {
                    // TODO: cancellation handling
                    progress.Report(new StatusBarProgressData(Resources.Info_MarkdownSendingInputFile.FormatInvariant(Path.GetFileName(inputFilePath)), 0));

                    var rmd = await fts.SendFileAsync(inputFilePath, true, null, CancellationToken.None);
                    progress.Report(new StatusBarProgressData(Resources.Info_MarkdownPublishingFile.FormatInvariant(Path.GetFileName(inputFilePath)), 1));

                    var publishResult = await session.EvaluateAsync<ulong>($"rtvs:::rmarkdown_publish(blob_id = {rmd.Id}, output_format = {format.ToRStringLiteral()}, encoding = 'cp{codePage}')", REvaluationKind.Normal);

                    progress.Report(new StatusBarProgressData(Resources.Info_MarkdownGetOutputFile.FormatInvariant(Path.GetFileName(outputFilePath)), 2));
                    await fts.FetchFileAsync(new RBlobInfo(publishResult), outputFilePath, true, null, CancellationToken.None);

                    progress.Report(new StatusBarProgressData(Resources.Info_MarkdownPublishComplete.FormatInvariant(Path.GetFileName(outputFilePath)), 3));
                }
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
