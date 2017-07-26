// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Plots;
using Microsoft.VisualStudio.R.Package.ExportDialog;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal class VsPlotExportDialog : IRPlotExportDialog {
        private readonly ICoreShell _shell;

        public VsPlotExportDialog(ICoreShell shell) {
            _shell = shell;
        }

        public ExportImageParameters ShowExportImageDialog(ExportArguments imageArguments, string filter, string initialPath = null, string title = null)
            => ShowSaveExportImageDialog(_shell.GetDialogOwnerWindow(), imageArguments, filter, initialPath, title);

        public ExportPdfParameters ShowExportPdfDialog(ExportArguments pdfArguements, string filter, string initialPath = null, string title = null) =>
            ShowSaveExportPdfDialog(_shell.GetDialogOwnerWindow(),pdfArguements, filter, initialPath, title);

        private ExportPdfParameters ShowSaveExportPdfDialog(IntPtr owner,ExportArguments pdfArguments, string filter, string initialPath = null, string title = null) {
            var exportPdfDialog = new ExportPDFDialog(pdfArguments);
            exportPdfDialog.ShowModal();

            var pdfParameters = exportPdfDialog.GetExportParameters();
            if (exportPdfDialog.DialogResult == true) {
                pdfParameters.FilePath = _shell.FileDialog().ShowSaveFileDialog(filter, initialPath, title);
                return pdfParameters;
            }
            return null;
        }

        private ExportImageParameters ShowSaveExportImageDialog(IntPtr owner, ExportArguments imageArguments,string filter, string initialPath = null, string title = null) {
            var exportImageDialog = new ExportImageDialog(imageArguments);
            exportImageDialog.ShowModal();

            var exportParameters = exportImageDialog.GetExportParameters();
            if(exportImageDialog.DialogResult == true) {
                exportParameters.FilePath = _shell.FileDialog().ShowSaveFileDialog(filter, initialPath, title);
                return exportParameters;
            }
            return null;
        }
   }
}