// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.Plots {
    public interface IRPlotExportDialogs {
        /// <summary>
        /// Show the export image dialog box.
        /// </summary>
        /// <param name="imageArguments"></param>
        /// <param name="filter"></param>
        /// <param name="initialPath"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        ExportImageParameters ShowExportImageDialog(ExportArguments imageArguments, string filter, string initialPath = null, string title = null);

        /// <summary>
        /// Show the export PDF dialog box.
        /// </summary>
        /// <param name="pdfArguments"></param>
        /// <param name="filter"></param>
        /// <param name="initialPath"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        ExportPdfParameters ShowExportPdfDialog(ExportArguments pdfArguments, string filter, string initialPath = null, string title = null);
    }
}
