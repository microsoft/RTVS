namespace Microsoft.VisualStudio.R.Package.Commands
{
    public static class RPackageCommandId
    {
        // General
        public const int icmdGoToFormattingOptions = 400;
        public const int icmdGoToRToolsOptions = 401;

        // REPL
        public const int icmdSendToRepl = 501;
        public const int icmdLoadWorkspace = 502;
        public const int icmdSaveWorkspace = 503;
        public const int icmdResetWorkspace = 504;
        public const int icmdSetWorkingDirectory = 505;

        // Packages
        public const int icmdInstallPackages = 601;

        // Plots
        public const int icmdOpenPlot = 701;
        public const int icmdSavePlot = 702;
        public const int icmdFixPlot = 703;
        public const int icmdExportPlot = 704;
        public const int icmdZoomInPlot = 705;
        public const int icmdZoomOutPlot = 706;
        public const int icmdCopyPlot = 707;

        // Data
        public const int icmdImportDataset = 801;
        public const int icmdImportDatasetUrl = 802;
        public const int icmdImportDatasetTextFile = 803;

        // Window management
        public const int icmdShowReplWindow = 901;
        public const int icmdShowPlotWindow = 902;
        public const int icmdShowVariableExplorerWindow = 903;
        public const int icmdShowHistoryWindow = 904;
        public const int icmdShowPackagesWindow = 905;

        // Publishing
        //public const int icmdPublishDialog = 1001;
        //public const int icmdPublishPreviewHtml = 1002;
        //public const int icmdPublishPreviewPdf = 1003;
        //public const int icmdPublishPreviewWord = 1004;
    }
}
