namespace Microsoft.VisualStudio.R.Package.Commands {
    public static class RPackageCommandId {
        public const int plotWindowToolBarId = 0x2000;

        // General
        public const int icmdGoToFormattingOptions = 400;
        public const int icmdGoToRToolsOptions = 401;
        public const int icmdGoToREditorOptions = 402;
        public const int icmdSendSmile = 403;
        public const int icmdSendFrown = 404;

        // REPL
        public const int icmdLoadWorkspace = 502;
        public const int icmdSaveWorkspace = 503;
        public const int icmdSetWorkingDirectory = 504;
        public const int icmdRestartR = 505;
        public const int icmdInterruptR = 506;
        public const int icmdAttachDebugger = 507;
        public const int icmdSourceRScript = 508;

        public const int icmdRexecuteReplCmd = 571;
        public const int icmdPasteReplCmd = 572;

        // Packages
        public const int icmdInstallPackages = 601;
        public const int icmdCheckForPackageUpdates = 602;

        // Plots
        public const int icmdOpenPlot = 701;
        public const int icmdSavePlot = 702;
        public const int icmdFixPlot = 703;
        public const int icmdExportPlot = 704;
        public const int icmdPrintPlot = 705;
        public const int icmdCopyPlot = 707;
        public const int icmdZoomInPlot = 708;
        public const int icmdZoomOutPlot = 709;

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
