// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Session {
    public static class RSessionEvaluationCommands {
        public static Task OptionsSetWidth(this IRSessionEvaluation evaluation, int width) {
            return evaluation.EvaluateNonReentrantAsync($"options(width=as.integer({width}))\n");
        }

        public static async Task<string> GetRUserDirectory(this IRSessionEvaluation evaluation) {
            var result = await evaluation.EvaluateAsync("Sys.getenv('R_USER')");
            return result.StringResult.Replace('/', '\\');
        }

        public static async Task<string> GetWorkingDirectory(this IRSessionEvaluation evaluation) {
            var result = await evaluation.EvaluateAsync("getwd()");
            return result.StringResult.Replace('/', '\\');
        }

        public static Task SetWorkingDirectory(this IRSessionEvaluation evaluation, string path) {
            return evaluation.EvaluateNonReentrantAsync($"setwd('{path.Replace('\\', '/')}')\n");
        }

        public static Task SetDefaultWorkingDirectory(this IRSessionEvaluation evaluation) {
            return evaluation.EvaluateNonReentrantAsync($"setwd('~')\n");
        }

        public static Task<REvaluationResult> LoadWorkspace(this IRSessionEvaluation evaluation, string path) {
            return evaluation.EvaluateNonReentrantAsync($"load('{path.Replace('\\', '/')}', .GlobalEnv)\n");
        }

        public static Task<REvaluationResult> SaveWorkspace(this IRSessionEvaluation evaluation, string path) {
            return evaluation.EvaluateNonReentrantAsync($"save.image(file='{path.Replace('\\', '/')}')\n");
        }

        public static Task<REvaluationResult> SetVsGraphicsDevice(this IRSessionEvaluation evaluation) {
            var script = @"
attach(as.environment(list(ide = function() { rtvs:::graphics.ide.new() })), name='rtvs::graphics::ide')
options(device='ide')
grDevices::deviceIsInteractive('ide')
";
            return evaluation.EvaluateAsync(script);
        }

        public static Task ResizePlot(this IRSessionInteraction evaluation, int width, int height) {
            var script = string.Format("rtvs:::graphics.ide.resize({0}, {1})\n", width, height);
            return evaluation.RespondAsync(script);
        }

        public static Task NextPlot(this IRSessionInteraction evaluation) {
            var script = "rtvs:::graphics.ide.nextplot()\n";
            return evaluation.RespondAsync(script);
        }

        public static Task PreviousPlot(this IRSessionInteraction evaluation) {
            var script = "rtvs:::graphics.ide.previousplot()\n";
            return evaluation.RespondAsync(script);
        }

        public static Task ClearPlotHistory(this IRSessionInteraction evaluation) {
            var script = "rtvs:::graphics.ide.clearplots()\n";
            return evaluation.RespondAsync(script);
        }

        public static Task RemoveCurrentPlot(this IRSessionInteraction evaluation) {
            var script = "rtvs:::graphics.ide.removeplot()\n";
            return evaluation.RespondAsync(script);
        }

        public static Task<REvaluationResult> PlotHistoryInfo(this IRSessionEvaluation evaluation) {
            var script = @"rtvs:::toJSON(rtvs:::graphics.ide.historyinfo())";
            return evaluation.EvaluateAsync(script, REvaluationKind.Json);
        }

        public static Task<REvaluationResult> ExportToBitmap(this IRSessionEvaluation evaluation, string deviceName, string outputFilePath, int widthInPixels, int heightInPixels) {
            string script = string.Format("rtvs:::graphics.ide.exportimage(\"{0}\", {1}, {2}, {3})", outputFilePath.Replace("\\", "/"), deviceName, widthInPixels, heightInPixels);
            return evaluation.EvaluateAsync(script);
        }

        public static Task<REvaluationResult> ExportToMetafile(this IRSessionEvaluation evaluation, string outputFilePath, double widthInInches, double heightInInches) {
            string script = string.Format("rtvs:::graphics.ide.exportimage(\"{0}\", win.metafile, {1}, {2})", outputFilePath.Replace("\\", "/"), widthInInches, heightInInches);
            return evaluation.EvaluateAsync(script);
        }

        public static Task<REvaluationResult> ExportToPdf(this IRSessionEvaluation evaluation, string outputFilePath, double widthInInches, double heightInInches, string paper) {
            string script = string.Format("rtvs:::graphics.ide.exportpdf(\"{0}\", {1}, {2}, '{3}')", outputFilePath.Replace("\\", "/"), widthInInches, heightInInches, paper);
            return evaluation.EvaluateAsync(script);
        }

        public static Task<REvaluationResult> SetVsCranSelection(this IRSessionEvaluation evaluation, string mirrorUrl) {
            var script =
@"    local({
        r <- getOption('repos')
        r['CRAN'] <- '" + mirrorUrl + @"'
        options(repos = r)})";

            return evaluation.EvaluateAsync(script);
        }

        public static Task<REvaluationResult> SetVsHelpRedirection(this IRSessionEvaluation evaluation) {
            var script =
@"options(help_type = 'html')
  options(browser = function(url) { 
      .Call('Microsoft.R.Host::Call.send_message', 'Browser', rtvs:::toJSON(url)) 
  })";
            return evaluation.EvaluateAsync(script);
        }

        public static Task<REvaluationResult> SetChangeDirectoryRedirection(this IRSessionEvaluation evaluation) {
            var script =
@"utils::assignInNamespace('setwd', function(dir) {
    old <- .Internal(setwd(dir))
    .Call('Microsoft.R.Host::Call.send_message', '~/', rtvs:::toJSON(dir))
    invisible(old)
  }, 'base')";
            return evaluation.EvaluateAsync(script);
        }

        private static Task<REvaluationResult> EvaluateNonReentrantAsync(this IRSessionEvaluation evaluation, FormattableString commandText) {
            return evaluation.EvaluateAsync(FormattableString.Invariant(commandText));
        }
    }
}