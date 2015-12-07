using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.RPackages.Mirrors;

namespace Microsoft.VisualStudio.R.Package.Repl.Session
{
    public static class RSessionEvaluationCommands {
        public static Task OptionsSetWidth(this IRSessionEvaluation evaluation, int width) {
            return evaluation.EvaluateNonReentrantAsync($"options(width=as.integer({width}))\n");
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
.rtvs.vsgdresize <- function(width, height) {
   .External('rtvs::External.ide_graphicsdevice_resize', width, height)
}
.rtvs.vsgd <- function() {
   .External('rtvs::External.ide_graphicsdevice_new')
}
.rtvs.vsgdexportimage <- function(filename, device) {
    dev.copy(device=device,filename=filename)
    dev.off()
}
.rtvs.vsgdexportpdf <- function(filename) {
    dev.copy(device=pdf,file=filename)
    dev.off()
}
.rtvs.vsgdnextplot <- function() {
   .External('rtvs::External.ide_graphicsdevice_next_plot')
}
.rtvs.vsgdpreviousplot <- function() {
   .External('rtvs::External.ide_graphicsdevice_previous_plot')
}
xaml <- function(filename, width, height) {
   .External('rtvs::External.xaml_graphicsdevice_new', filename, width, height)
}
options(device='.rtvs.vsgd')
";

            return evaluation.EvaluateAsync(script);
        }

        public static Task<REvaluationResult> ResizePlot(this IRSessionEvaluation evaluation, int width, int height) {
            var script = string.Format(".rtvs.vsgdresize({0}, {1})", width, height);
            return evaluation.EvaluateAsync(script);
        }

        public static Task<REvaluationResult> NextPlot(this IRSessionEvaluation evaluation) {
            var script = @".rtvs.vsgdnextplot()";
            return evaluation.EvaluateAsync(script);
        }

        public static Task<REvaluationResult> PreviousPlot(this IRSessionEvaluation evaluation) {
            var script = @".rtvs.vsgdpreviousplot()";
            return evaluation.EvaluateAsync(script);
        }

        public static Task<REvaluationResult> CopyToDevice(this IRSessionEvaluation evaluation, string deviceName, string outputFilePath) {
            string script;
            switch (deviceName) {
                case "pdf":
                    script = string.Format(".rtvs.vsgdexportpdf('{0}')", outputFilePath.Replace("\\", "/"));
                    break;

                default:
                    script = string.Format(".rtvs.vsgdexportimage('{0}', {1})", outputFilePath.Replace("\\", "/"), deviceName);
                    break;
            }
            return evaluation.EvaluateAsync(script);
        }

        public static Task<REvaluationResult> SetVsCranSelection(this IRSessionEvaluation evaluation, string mirrorUrl)
        {
            var script = 
@"    local({
        r <- getOption('repos')
        r['CRAN'] <- '" + mirrorUrl + @"'
        options(repos = r)})";

            return evaluation.EvaluateAsync(script);
        }

        public static Task<REvaluationResult> SetVsHelpRedirection(this IRSessionEvaluation evaluation) {
            var script =
@"options(browser = function(url) { 
      .Call('rtvs::Call.send_message', 'Browser', rtvs:::toJSON(url)) 
  })";
            return evaluation.EvaluateAsync(script);
        }

        private static Task<REvaluationResult> EvaluateNonReentrantAsync(this IRSessionEvaluation evaluation, FormattableString commandText) {
            return evaluation.EvaluateAsync(FormattableString.Invariant(commandText));
        }
    }
}