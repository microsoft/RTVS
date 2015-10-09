using System;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Repl.Session {
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

        public static Task<REvaluationResult> GetGlobalEnvironmentVariables(this IRSessionEvaluation evaluation) {
            return evaluation.EvaluateNonReentrantAsync($".rtvs.datainspect.env_vars(.GlobalEnv)\n");
        }

        public static Task<REvaluationResult> SetVsGraphicsDevice(this IRSessionEvaluation evaluation) {
            var script = @"
.rtvs.vsgd <- function() {
   .External('C_vsgd', 5, 5)
}
options(device='.rtvs.vsgd')
";

            return evaluation.EvaluateAsync(script, reentrant: false);
        }

        private static Task<REvaluationResult> EvaluateNonReentrantAsync(this IRSessionEvaluation evaluation, FormattableString commandText) {
            return evaluation.EvaluateAsync(FormattableString.Invariant(commandText), reentrant: false);
        }
    }
}