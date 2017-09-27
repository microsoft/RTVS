// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Host.Client.Session {
    public static class RSessionEvaluationCommands {
        public static Task OptionsSetWidthAsync(this IRExpressionEvaluator evaluation, int width) 
            => evaluation.ExecuteAsync(Invariant($"options(width=as.integer({width}))\n"));

        public static Task QuitAsync(this IRExpressionEvaluator eval) =>
            eval.ExecuteAsync("q()", REvaluationKind.Normal);

        public static async Task<string> GetRUserDirectoryAsync(this IRExpressionEvaluator evaluation, CancellationToken cancellationToken = default(CancellationToken)) {
            var result = await evaluation.EvaluateAsync<string>("Sys.getenv('R_USER')", REvaluationKind.Normal, cancellationToken);
            return result.Replace('/', '\\');
        }

        public static async Task<string> GetWorkingDirectoryAsync(this IRExpressionEvaluator evaluation) {
            var result = await evaluation.EvaluateAsync<string>("getwd()", REvaluationKind.Normal);
            return result.Replace('/', '\\');
        }

        public static Task SetWorkingDirectoryAsync(this IRExpressionEvaluator evaluation, string path) {
            return evaluation.ExecuteAsync(Invariant($"setwd('{path.Replace('\\', '/')}')\n"));
        }

        public static Task SetDefaultWorkingDirectoryAsync(this IRExpressionEvaluator evaluation) {
            return evaluation.ExecuteAsync($"setwd('~')\n");
        }

        public static Task LoadWorkspaceAsync(this IRExpressionEvaluator evaluation, string path) {
            return evaluation.ExecuteAsync(Invariant($"load('{path.Replace('\\', '/')}', .GlobalEnv)\n"));
        }

        public static Task SaveWorkspaceAsync(this IRExpressionEvaluator evaluation, string path) {
            return evaluation.ExecuteAsync(Invariant($"save.image(file='{path.Replace('\\', '/')}')\n"), REvaluationKind.Normal);
        }

        public static Task SetVsGraphicsDeviceAsync(this IRExpressionEvaluator evaluation) {
            var script = @"
attach(as.environment(list(ide = function() { rtvs:::graphics.ide.new() })), name='rtvs::graphics::ide')
options(device='ide')
grDevices::deviceIsInteractive('ide')
";
            return evaluation.ExecuteAsync(script);
        }

        public static Task ResizePlotAsync(this IRExpressionEvaluator evaluation, Guid deviceId, int width, int height, int resolution) {
            Debug.Assert(deviceId != Guid.Empty);
            var script = Invariant($"rtvs:::graphics.ide.resize({deviceId.ToString().ToRStringLiteral()}, {width}, {height}, {resolution})\n");
            return evaluation.ExecuteAsync(script);
        }

        public static Task NextPlotAsync(this IRExpressionEvaluator evaluation, Guid deviceId) {
            Debug.Assert(deviceId != Guid.Empty);
            var script = Invariant($"rtvs:::graphics.ide.nextplot({deviceId.ToString().ToRStringLiteral()})\n");
            return evaluation.ExecuteAsync(script);
        }

        public static Task PreviousPlotAsync(this IRExpressionEvaluator evaluation, Guid deviceId) {
            Debug.Assert(deviceId != Guid.Empty);
            var script = Invariant($"rtvs:::graphics.ide.previousplot({deviceId.ToString().ToRStringLiteral()})\n");
            return evaluation.ExecuteAsync(script);
        }

        public static Task ClearPlotHistoryAsync(this IRExpressionEvaluator evaluation, Guid deviceId) {
            Debug.Assert(deviceId != Guid.Empty);
            var script = Invariant($"rtvs:::graphics.ide.clearplots({deviceId.ToString().ToRStringLiteral()})\n");
            return evaluation.ExecuteAsync(script);
        }

        public static Task RemoveCurrentPlotAsync(this IRExpressionEvaluator evaluation, Guid deviceId, Guid plotId) {
            Debug.Assert(deviceId != Guid.Empty);
            Debug.Assert(plotId != Guid.Empty);
            var script = Invariant($"rtvs:::graphics.ide.removeplot({deviceId.ToString().ToRStringLiteral()}, {plotId.ToString().ToRStringLiteral()})\n");
            return evaluation.ExecuteAsync(script);
        }

        public static Task CopyPlotAsync(this IRExpressionEvaluator evaluation, Guid sourceDeviceId, Guid sourcePlotId, Guid targetDeviceId) {
            Debug.Assert(sourceDeviceId != Guid.Empty);
            Debug.Assert(sourcePlotId != Guid.Empty);
            Debug.Assert(targetDeviceId != Guid.Empty);
            var script = Invariant($"rtvs:::graphics.ide.copyplot({sourceDeviceId.ToString().ToRStringLiteral()}, {sourcePlotId.ToString().ToRStringLiteral()}, {targetDeviceId.ToString().ToRStringLiteral()})\n");
            return evaluation.ExecuteAsync(script);
        }

        public static Task SelectPlotAsync(this IRExpressionEvaluator evaluation, Guid deviceId, Guid plotId) {
            Debug.Assert(deviceId != Guid.Empty);
            Debug.Assert(plotId != Guid.Empty);
            var script = Invariant($"rtvs:::graphics.ide.selectplot({deviceId.ToString().ToRStringLiteral()}, {plotId.ToString().ToRStringLiteral()})\n");
            return evaluation.ExecuteAsync(script);
        }

        public static async Task ActivatePlotDeviceAsync(this IRExpressionEvaluator evaluation, Guid deviceId) {
            Debug.Assert(deviceId != Guid.Empty);
            await evaluation.ExecuteAsync(Invariant($"rtvs:::graphics.ide.setactivedeviceid({deviceId.ToString().ToRStringLiteral()})"));
        }

        public static Task<int?> GetPlotDeviceNumAsync(this IRExpressionEvaluator evaluation, Guid deviceId) {
            Debug.Assert(deviceId != Guid.Empty);
            return evaluation.EvaluateAsync<int?>(Invariant($"rtvs:::graphics.ide.getdevicenum({deviceId.ToString().ToRStringLiteral()})"), REvaluationKind.Normal);
        }

        public static async Task NewPlotDeviceAsync(this IRExpressionEvaluator evaluation) {
            await evaluation.ExecuteAsync("ide()");
        }

        public static async Task<Guid> GetActivePlotDeviceAsync(this IRExpressionEvaluator evaluation) {
            var id = await evaluation.EvaluateAsync<string>("rtvs:::graphics.ide.getactivedeviceid()", REvaluationKind.Normal);
            return id != null ? Guid.Parse(id) : Guid.Empty;
        }

        public static Task InstallPackageAsync(this IRSessionInteraction interaction, string name) {
            var script = Invariant($"install.packages({name.ToRStringLiteral()})\n");
            return interaction.RespondAsync(script);
        }

        public static Task InstallPackageAsync(this IRSessionInteraction interaction, string name, string libraryPath) {
            var script = Invariant($"install.packages({name.ToRStringLiteral()}, lib={libraryPath.ToRPath().ToRStringLiteral()})\n");
            return interaction.RespondAsync(script);
        }

        public static Task UninstallPackageAsync(this IRSessionInteraction interaction, string name) {
            var script = Invariant($"remove.packages({name.ToRStringLiteral()})\n");
            return interaction.RespondAsync(script);
        }

        public static Task UninstallPackageAsync(this IRSessionInteraction interaction, string name, string libraryPath) {
            var script = Invariant($"remove.packages({name.ToRStringLiteral()}, lib={libraryPath.ToRPath().ToRStringLiteral()})\n");
            return interaction.RespondAsync(script);
        }

        public static Task LoadPackageAsync(this IRSessionInteraction interaction, string name) {
            var script = Invariant($"library({name.ToRStringLiteral()})\n");
            return interaction.RespondAsync(script);
        }

        public static Task LoadPackageAsync(this IRSessionInteraction interaction, string name, string libraryPath) {
            var script = Invariant($"library({name.ToRStringLiteral()}, lib.loc={libraryPath.ToRPath().ToRStringLiteral()})\n");
            return interaction.RespondAsync(script);
        }

        public static Task UnloadPackageAsync(this IRSessionInteraction interaction, string name) {
            var script = Invariant($"unloadNamespace({name.ToRStringLiteral()})\n");
            return interaction.RespondAsync(script);
        }

        public static Task<JArray> InstalledPackagesAsync(this IRExpressionEvaluator evaluation) {
            var script = "rtvs:::packages.installed()";
            return evaluation.EvaluateAsync<JArray>(script, REvaluationKind.Normal);
        }

        public static Task<JArray> InstalledPackagesFunctionsAsync(this IRExpressionEvaluator evaluation, REvaluationKind kind = REvaluationKind.Normal, CancellationToken cancellationToken = default(CancellationToken)) {
            var script = "rtvs:::packages.installed.functions()";
            return evaluation.EvaluateAsync<JArray>(script, kind, cancellationToken);
        }

        public static Task<JArray> PackageExportedFunctionsNamesAsync(this IRExpressionEvaluator evaluation, string packageName, REvaluationKind kind = REvaluationKind.Normal, CancellationToken cancellationToken = default(CancellationToken)) {
            var script = Invariant($"rtvs:::package.exported.functions.names({packageName.ToRStringLiteral()})");
            return evaluation.EvaluateAsync<JArray>(script, kind, cancellationToken);
        }

        public static Task<JArray> PackageAllFunctionsNamesAsync(this IRExpressionEvaluator evaluation, string packageName, REvaluationKind kind = REvaluationKind.Normal, CancellationToken cancellationToken = default(CancellationToken)) {
            var script = Invariant($"rtvs:::package.all.functions.names({packageName.ToRStringLiteral()})");
            return evaluation.EvaluateAsync<JArray>(script, kind, cancellationToken);
        }

        public static Task<JArray> AvailablePackagesAsync(this IRExpressionEvaluator evaluation) {
            var script = @"rtvs:::packages.available()";
            return evaluation.EvaluateAsync<JArray>(script, REvaluationKind.Reentrant);
        }

        public static Task<JArray> LoadedPackagesAsync(this IRExpressionEvaluator evaluation, CancellationToken cancellationToken = default(CancellationToken)) {
            var script = @"rtvs:::packages.loaded()";
            return evaluation.EvaluateAsync<JArray>(script, REvaluationKind.Normal, cancellationToken);
        }

        public static Task<string[]> LibraryPathsAsync(this IRExpressionEvaluator evaluation, CancellationToken cancellationToken = default(CancellationToken)) {
            var script = @"rtvs:::packages.libpaths()";
            return evaluation.EvaluateAsync<string[]>(script, REvaluationKind.Normal, cancellationToken);
        }
        public static Task<ulong> ExportPlotToBitmapAsync(this IRExpressionEvaluator evaluation, Guid deviceId, Guid plotId, string deviceName, int widthInPixels, int heightInPixels, int resolution) {
            var script = Invariant($"rtvs:::export_to_image({deviceId.ToString().ToRStringLiteral()}, {plotId.ToString().ToRStringLiteral()}, {deviceName}, {widthInPixels}, {heightInPixels}, {resolution})");
            return evaluation.EvaluateAsync<ulong>(script, REvaluationKind.Normal);
        }

        public static Task<ulong> ExportPlotToMetafileAsync(this IRExpressionEvaluator evaluation, Guid deviceId, Guid plotId, double widthInInches, double heightInInches, int resolution) {
            var script = Invariant($"rtvs:::export_to_image({deviceId.ToString().ToRStringLiteral()}, {plotId.ToString().ToRStringLiteral()}, win.metafile, {widthInInches}, {heightInInches}, {resolution})");
            return evaluation.EvaluateAsync<ulong>(script, REvaluationKind.Normal);
        }

        public static Task<ulong> ExportToPdfAsync(this IRExpressionEvaluator evaluation, Guid deviceId, Guid plotId, string pdfDevice, string paper, double inchWidth, double inchHeight) {
            return (pdfDevice == "cairo_pdf") ?
                ExportToCairoPdfAsync(evaluation, deviceId, plotId, pdfDevice, inchWidth, inchHeight) :
                ExportToDefaultPdfAsync(evaluation, deviceId, plotId, pdfDevice, paper, inchWidth, inchHeight);
        }

        private static Task<ulong> ExportToCairoPdfAsync(this IRExpressionEvaluator evaluation, Guid deviceId, Guid plotId, string pdfDevice, double inchWidth, double inchHeight) {
            string script = Invariant($"rtvs:::export_to_pdf({deviceId.ToString().ToRStringLiteral()}, {plotId.ToString().ToRStringLiteral()}, {pdfDevice}, {inchWidth}, {inchHeight})");
            return evaluation.EvaluateAsync<ulong>(script, REvaluationKind.Normal);
        }

        private static Task<ulong> ExportToDefaultPdfAsync(this IRExpressionEvaluator evaluation, Guid deviceId, Guid plotId, string pdfDevice, string paper, double inchWidth, double inchHeight) {
            string script = Invariant($"rtvs:::export_to_pdf({deviceId.ToString().ToRStringLiteral()}, {plotId.ToString().ToRStringLiteral()}, {pdfDevice}, {inchWidth}, {inchHeight}, {paper.ToRStringLiteral()})");
            return evaluation.EvaluateAsync<ulong>(script, REvaluationKind.Normal);
        }

        public static async Task SetVsCranSelectionAsync(this IRExpressionEvaluator evaluation, string mirrorUrl, CancellationToken cancellationToken = default(CancellationToken)) {
            await evaluation.ExecuteAsync(Invariant($"rtvs:::set_mirror({mirrorUrl.ToRStringLiteral()})"), cancellationToken);
        }

        public static Task SetROptionsAsync(this IRExpressionEvaluator evaluation) {
            var script =
@"options(help_type = 'html')
  options(browser = rtvs:::open_url)
  options(pager = rtvs:::show_file)
  options(editor = rtvs:::edit_file)
";
            return evaluation.ExecuteAsync(script);
        }

        public static Task<string> GetRSessionPlatformAsync(this IRExpressionEvaluator evaluation, CancellationToken cancellationToken = default(CancellationToken)) {
            var script = Invariant($".Platform$OS.type");
            return evaluation.EvaluateAsync<string>(script, REvaluationKind.Normal, cancellationToken);
        }

        public static async Task<bool> IsRSessionPlatformWindowsAsync(this IRExpressionEvaluator evaluation, CancellationToken cancellationToken = default(CancellationToken)) {
            var platformType = await evaluation.GetRSessionPlatformAsync(cancellationToken);
            return platformType.EqualsIgnoreCase("windows");
        }

        public static async Task SetCodePageAsync(this IRExpressionEvaluator evaluation, int codePage, CancellationToken cancellationToken = default(CancellationToken)) {
            string cp = null;
            if (codePage == 0) {
                // Non-Windows defaults to UTF-8, on Windows leave default alone.
                if (!await evaluation.IsRSessionPlatformWindowsAsync(cancellationToken)) {
                    cp = "en_US.UTF-8";
                }
            }
            else {
                cp = Invariant($".{codePage}");
            }

            if (!string.IsNullOrEmpty(cp)) {
                var script = Invariant($"Sys.setlocale('LC_CTYPE', '{cp}')");
                await evaluation.ExecuteAsync(script, cancellationToken);
            }
        }

        public static Task OverrideFunctionAsync(this IRExpressionEvaluator evaluation, string name, string ns) {
            name = name.ToRStringLiteral();
            ns = ns.ToRStringLiteral();
            var script = Invariant($"utils::assignInNamespace({name}, rtvs:::{name}, {ns})");
            return evaluation.ExecuteAsync(script);
        }

        public static Task SetFunctionRedirectionAsync(this IRExpressionEvaluator evaluation) {
            var script = "rtvs:::redirect_functions()";
            return evaluation.ExecuteAsync(script);
        }

        public static Task SetGridEvalModeAsync(this IRExpressionEvaluator evaluation, bool dynamicEval) {
            var script = Invariant($"rtvs:::set_view_mode({(dynamicEval ? 1 : 0)})");
            return evaluation.ExecuteAsync(script);
        }

        public static Task<bool> QueryReloadAutosaveAsync(this IRExpressionEvaluator evaluation) =>
            evaluation.EvaluateAsync<bool>($"rtvs:::query_reload_autosave()", REvaluationKind.Reentrant);

        public static Task EnableAutosaveAsync(this IRExpressionEvaluator evaluation, bool deleteExisting) =>
            evaluation.ExecuteAsync(Invariant($"rtvs:::enable_autosave({deleteExisting.ToRBooleanLiteral()})"));

        public static Task<bool> FileExistsAsync(this IRExpressionEvaluator evaluation, string path, CancellationToken cancellationToken = default(CancellationToken)) =>
            evaluation.EvaluateAsync<bool>(Invariant($"file.exists({path.ToRPath().ToRStringLiteral()})"), REvaluationKind.Normal, cancellationToken);

        public static Task<string> NormalizePathAsync(this IRExpressionEvaluator evaluation, string path, CancellationToken cancellationToken = default(CancellationToken)) =>
            evaluation.EvaluateAsync<string>(Invariant($"normalizePath({path.ToRPath().ToRStringLiteral()})"), REvaluationKind.Normal, cancellationToken);
    }
}