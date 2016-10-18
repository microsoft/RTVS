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
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

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
                IApplicationShell appShell = coreShell as IApplicationShell;
                IVsStatusbar statusBar = appShell?.GetGlobalService<IVsStatusbar>(typeof(SVsStatusbar));
                await RMarkdownRenderAsync(session, fs, inputFilePath, outputFilePath, GetDocumentTypeString(publishFormat), encoding.CodePage, statusBar);
            } catch (IOException ex) {
                coreShell.ShowErrorMessage(ex.Message);
            } catch (RException ex) {
                coreShell.ShowErrorMessage(ex.Message);
            } catch (OperationCanceledException) {
            } 
        }

        private async Task RMarkdownRenderAsync(IRSession session, IFileSystem fs, string inputFilePath, string outputFilePath, string format, int codePage, IVsStatusbar statusBar) {
            using (var fts = new DataTransferSession(session, fs)) {
                string currentStatusText;
                statusBar.GetText(out currentStatusText);
                uint cookie = 0;
                statusBar.Progress(ref cookie, 1, "", 0, 0);

                try {
                    statusBar.Progress(ref cookie, 1, string.Format(Resources.Info_MarkdownSendingInputFile, Path.GetFileName(inputFilePath)), 0, 3);
                    var rmd = await fts.SendFileAsync(inputFilePath);
                    statusBar.Progress(ref cookie, 1, string.Format(Resources.Info_MarkdownPublishingFile, Path.GetFileName(inputFilePath)), 1, 3);
                    var publishResult = await session.EvaluateAsync<ulong>($"rtvs:::rmarkdown_publish(blob_id = {rmd.Id}, output_format = {format.ToRStringLiteral()}, encoding = 'cp{codePage}')", REvaluationKind.Normal);
                    statusBar.Progress(ref cookie, 1, string.Format(Resources.Info_MarkdownGetOutputFile, Path.GetFileName(inputFilePath)), 2, 3);
                    await fts.FetchFileAsync(new RBlobInfo(publishResult), outputFilePath);
                    statusBar.Progress(ref cookie, 1, string.Format(Resources.Info_MarkdownPublishComplete, Path.GetFileName(inputFilePath)), 3, 3);
                } finally {
                    statusBar.Progress(ref cookie, 0, "", 0, 0);
                    statusBar.SetText(currentStatusText);
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
