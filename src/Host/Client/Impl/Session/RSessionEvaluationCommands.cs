// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using static System.FormattableString;

namespace Microsoft.R.Host.Client.Session {
    public static class RSessionEvaluationCommands {
        public static Task OptionsSetWidth(this IRExpressionEvaluator evaluation, int width) {
            return evaluation.EvaluateAsync(Invariant($"options(width=as.integer({width}))\n"), REvaluationKind.Mutating);
        }

        public static async Task<string> GetRUserDirectory(this IRExpressionEvaluator evaluation) {
            var result = await evaluation.EvaluateAsync<string>("Sys.getenv('R_USER')", REvaluationKind.Json);
            return result.Replace('/', '\\');
        }

        public static async Task<string> GetWorkingDirectory(this IRExpressionEvaluator evaluation) {
            var result = await evaluation.EvaluateAsync<string>("getwd()", REvaluationKind.Json);
            return result.Replace('/', '\\');
        }

        public static Task SetWorkingDirectory(this IRExpressionEvaluator evaluation, string path) {
            return evaluation.EvaluateAsync(Invariant($"setwd('{path.Replace('\\', '/')}')\n"), REvaluationKind.Normal);
        }

        public static Task SetDefaultWorkingDirectory(this IRExpressionEvaluator evaluation) {
            return evaluation.EvaluateAsync($"setwd('~')\n", REvaluationKind.Normal);
        }

        public static Task<REvaluationResult> LoadWorkspace(this IRExpressionEvaluator evaluation, string path) {
            return evaluation.EvaluateAsync(Invariant($"load('{path.Replace('\\', '/')}', .GlobalEnv)\n"), REvaluationKind.Mutating);
        }

        public static Task<REvaluationResult> SaveWorkspace(this IRExpressionEvaluator evaluation, string path) {
            return evaluation.EvaluateAsync(Invariant($"save.image(file='{path.Replace('\\', '/')}')\n"), REvaluationKind.Normal);
        }

        public static Task<REvaluationResult> SetVsGraphicsDevice(this IRExpressionEvaluator evaluation) {
            var script = @"
attach(as.environment(list(ide = function() { rtvs:::graphics.ide.new() })), name='rtvs::graphics::ide')
options(device='ide')
grDevices::deviceIsInteractive('ide')
";
            return evaluation.EvaluateAsync(script, REvaluationKind.Normal);
        }

        public static Task ResizePlot(this IRSessionInteraction evaluation, int width, int height, int resolution) {
            var script = Invariant($"rtvs:::graphics.ide.resize({width}, {height}, {resolution})\n");
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

        public static Task<REvaluationResult> PlotHistoryInfo(this IRExpressionEvaluator evaluation) {
            var script = @"rtvs:::graphics.ide.historyinfo()";
            return evaluation.EvaluateAsync(script, REvaluationKind.Json);
        }

        public static Task InstallPackage(this IRSessionInteraction interaction, string name) {
            var script = Invariant($"install.packages({name.ToRStringLiteral()})\n");
            return interaction.RespondAsync(script);
        }

        public static Task InstallPackage(this IRSessionInteraction interaction, string name, string libraryPath) {
            var script = Invariant($"install.packages({name.ToRStringLiteral()}, lib={libraryPath.ToRPath().ToRStringLiteral()})\n");
            return interaction.RespondAsync(script);
        }

        public static Task UninstallPackage(this IRSessionInteraction interaction, string name) {
            var script = Invariant($"remove.packages({name.ToRStringLiteral()})\n");
            return interaction.RespondAsync(script);
        }

        public static Task UninstallPackage(this IRSessionInteraction interaction, string name, string libraryPath) {
            var script = Invariant($"remove.packages({name.ToRStringLiteral()}, lib={libraryPath.ToRPath().ToRStringLiteral()})\n");
            return interaction.RespondAsync(script);
        }

        public static Task LoadPackage(this IRSessionInteraction interaction, string name) {
            var script = Invariant($"library({name.ToRStringLiteral()})\n");
            return interaction.RespondAsync(script);
        }

        public static Task LoadPackage(this IRSessionInteraction interaction, string name, string libraryPath) {
            var script = Invariant($"library({name.ToRStringLiteral()}, lib.loc={libraryPath.ToRPath().ToRStringLiteral()})\n");
            return interaction.RespondAsync(script);
        }

        public static Task UnloadPackage(this IRSessionInteraction interaction, string name) {
            var script = Invariant($"unloadNamespace({name.ToRStringLiteral()})\n");
            return interaction.RespondAsync(script);
        }

        public static Task<REvaluationResult> InstalledPackages(this IRExpressionEvaluator evaluation) {
            var script = @"rtvs:::packages.installed()";
            return evaluation.EvaluateAsync(script, REvaluationKind.Json);
        }

        public static Task<REvaluationResult> AvailablePackages(this IRExpressionEvaluator evaluation) {
            var script = @"rtvs:::packages.available()";
            return evaluation.EvaluateAsync(script, REvaluationKind.Json | REvaluationKind.Reentrant);
        }

        public static Task<REvaluationResult> LoadedPackages(this IRExpressionEvaluator evaluation) {
            var script = @"rtvs:::packages.loaded()";
            return evaluation.EvaluateAsync(script, REvaluationKind.Json);
        }

        public static Task<REvaluationResult> LibraryPaths(this IRExpressionEvaluator evaluation) {
            var script = @"rtvs:::packages.libpaths()";
            return evaluation.EvaluateAsync(script, REvaluationKind.Json);
        }

        public static Task<REvaluationResult> ExportToBitmap(this IRExpressionEvaluator evaluation, string deviceName, string outputFilePath, int widthInPixels, int heightInPixels, int resolution) {
            string script = Invariant($"rtvs:::graphics.ide.exportimage({outputFilePath.ToRPath().ToRStringLiteral()}, {deviceName}, {widthInPixels}, {heightInPixels}, {resolution})");
            return evaluation.EvaluateAsync(script, REvaluationKind.Normal);
        }

        public static Task<REvaluationResult> ExportToMetafile(this IRExpressionEvaluator evaluation, string outputFilePath, double widthInInches, double heightInInches, int resolution) {
            string script = Invariant($"rtvs:::graphics.ide.exportimage({outputFilePath.ToRPath().ToRStringLiteral()}, win.metafile, {widthInInches}, {heightInInches}, {resolution})");
            return evaluation.EvaluateAsync(script, REvaluationKind.Normal);
        }

        public static Task<REvaluationResult> ExportToPdf(this IRExpressionEvaluator evaluation, string outputFilePath, double widthInInches, double heightInInches) {
            string script = Invariant($"rtvs:::graphics.ide.exportpdf({outputFilePath.ToRPath().ToRStringLiteral()}, {widthInInches}, {heightInInches})");
            return evaluation.EvaluateAsync(script, REvaluationKind.Normal);
        }

        public static async Task SetVsCranSelection(this IRExpressionEvaluator evaluation, string mirrorUrl) {
            await evaluation.EvaluateAsync(Invariant($"rtvs:::set_mirror({mirrorUrl.ToRStringLiteral()})"), REvaluationKind.Mutating);
        }

        public static Task<REvaluationResult> SetROptions(this IRExpressionEvaluator evaluation) {
            var script =
@"options(help_type = 'html')
  options(browser = rtvs:::open_url)
  options(pager = rtvs:::show_file)
";
            return evaluation.EvaluateAsync(script, REvaluationKind.Mutating);
        }

        public static Task SetCodePage(this IRExpressionEvaluator evaluation, int codePage) {
            if (codePage == 0) {
                codePage = NativeMethods.GetOEMCP();
            }
            var script = Invariant($"Sys.setlocale('LC_CTYPE', '.{codePage}')");
            return evaluation.EvaluateAsync(script, REvaluationKind.Mutating);
        }

        public static Task<REvaluationResult> OverrideFunction(this IRExpressionEvaluator evaluation, string name, string ns) {
            name = name.ToRStringLiteral();
            ns = ns.ToRStringLiteral();
            var script = Invariant($"utils::assignInNamespace({name}, rtvs:::{name}, {ns})");
            return evaluation.EvaluateAsync(script, REvaluationKind.Mutating);
        }


        public static Task<REvaluationResult> SetFunctionRedirection(this IRExpressionEvaluator evaluation) {
            var script = "rtvs:::redirect_functions()";
            return evaluation.EvaluateAsync(script, REvaluationKind.Mutating);
        }
    }
}