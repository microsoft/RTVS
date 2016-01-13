using System;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Session {
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
   invisible(.External('Microsoft.R.Host::External.ide_graphicsdevice_resize', width, height))
}
.rtvs.vsgd <- function() {
   invisible(.External('Microsoft.R.Host::External.ide_graphicsdevice_new'))
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
   invisible(.External('Microsoft.R.Host::External.ide_graphicsdevice_next_plot'))
}
.rtvs.vsgdpreviousplot <- function() {
   invisible(.External('Microsoft.R.Host::External.ide_graphicsdevice_previous_plot'))
}
.rtvs.vsgdhistoryinfo <- function() {
   .External('Microsoft.R.Host::External.ide_graphicsdevice_history_info')
}
xaml <- function(filename, width, height) {
   invisible(.External('Microsoft.R.Host::External.xaml_graphicsdevice_new', filename, width, height))
}
options(device='.rtvs.vsgd')
";

            return evaluation.EvaluateAsync(script);
        }

        public static Task ResizePlot(this IRSessionInteraction evaluation, int width, int height) {
            var script = string.Format(".rtvs.vsgdresize({0}, {1})\n", width, height);
            return evaluation.RespondAsync(script);
        }

        public static Task NextPlot(this IRSessionInteraction evaluation) {
            var script = ".rtvs.vsgdnextplot()\n";
            return evaluation.RespondAsync(script);
        }

        public static Task PreviousPlot(this IRSessionInteraction evaluation) {
            var script = ".rtvs.vsgdpreviousplot()\n";
            return evaluation.RespondAsync(script);
        }

        public static Task<REvaluationResult> PlotHistoryInfo(this IRSessionEvaluation evaluation) {
            var script = @"rtvs:::toJSON(.rtvs.vsgdhistoryinfo())";
            return evaluation.EvaluateAsync(script, REvaluationKind.Json);
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
@"options(browser = function(url) { 
      .Call('Microsoft.R.Host::Call.send_message', 'Browser', rtvs:::toJSON(url)) 
  })";
            return evaluation.EvaluateAsync(script);
        }

        public static Task<REvaluationResult> SetRdHelpExtraction(this IRSessionEvaluation evaluation) {
            var script =
@" .rtvs.signature.help2 <- function(f, p) {
        x <- help(paste(f), paste(p))
        y <- utils:::.getHelpFile(x)
        paste0(y, collapse = '')
    }

    .rtvs.signature.help1 <- function(f) {
        x <- help(paste(f))
        y <- utils:::.getHelpFile(x)
        paste0(y, collapse = '')
    }";
            return evaluation.EvaluateAsync(script);
        }

        public static Task<REvaluationResult> SetChangeDirectoryRedirection(this IRSessionEvaluation evaluation) {
            var script =
@"utils::assignInNamespace('setwd', function(dir) {
    .Internal(setwd(dir))
    .Call('Microsoft.R.Host::Call.send_message', '~/', rtvs:::toJSON(dir))
  }, 'base')";
            return evaluation.EvaluateAsync(script);
        }

        private static Task<REvaluationResult> EvaluateNonReentrantAsync(this IRSessionEvaluation evaluation, FormattableString commandText) {
            return evaluation.EvaluateAsync(FormattableString.Invariant(commandText));
        }
    }
}