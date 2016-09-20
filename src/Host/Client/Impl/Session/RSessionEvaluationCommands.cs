// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static System.FormattableString;
using System;
using System.Diagnostics;

namespace Microsoft.R.Host.Client.Session {
    public static class RSessionEvaluationCommands {
        public static Task OptionsSetWidthAsync(this IRExpressionEvaluator evaluation, int width) {
            return evaluation.ExecuteAsync(Invariant($"options(width=as.integer({width}))\n"));
        }

        public static async Task<string> GetRUserDirectoryAsync(this IRExpressionEvaluator evaluation) {
            var result = await evaluation.EvaluateAsync<string>("Sys.getenv('R_USER')", REvaluationKind.Normal);
            return result.Replace('/', '\\');
        }

        public static async Task<string> GetWorkingDirectoryAsync(this IRExpressionEvaluator evaluation) {
            var result = await evaluation.EvaluateAsync<string>("getwd()", REvaluationKind.Normal);
            return result.Replace('/', '\\');
        }

        public static Task SetWorkingDirectoryAsync(this IRExpressionEvaluator evaluation, string path) {
            return evaluation.ExecuteAsync($"setwd('{path.Replace('\\', '/')}')\n");
        }

        public static Task SetDefaultWorkingDirectoryAsync(this IRExpressionEvaluator evaluation) {
            return evaluation.ExecuteAsync($"setwd('~')\n");
        }

        public static Task LoadWorkspaceAsync(this IRExpressionEvaluator evaluation, string path) {
            return evaluation.ExecuteAsync($"load('{path.Replace('\\', '/')}', .GlobalEnv)\n");
        }

        public static Task SaveWorkspaceAsync(this IRExpressionEvaluator evaluation, string path) {
            return evaluation.ExecuteAsync($"save.image(file='{path.Replace('\\', '/')}')\n", REvaluationKind.Normal);
        }

        public static Task SetVsGraphicsDeviceAsync(this IRExpressionEvaluator evaluation)
        {
            var script = @"
attach(as.environment(list(ide = function() { rtvs:::graphics.ide.new() })), name='rtvs::graphics::ide')
options(device='ide')
grDevices::deviceIsInteractive('ide')
";
            return evaluation.ExecuteAsync(script);
        }

        public static Task ResizePlotAsync(this IRExpressionEvaluator evaluation, Guid deviceId, int width, int height, int resolution)
        {
            Debug.Assert(deviceId != Guid.Empty);
            var script = $"rtvs:::graphics.ide.resize({deviceId.ToString().ToRStringLiteral()}, {width}, {height}, {resolution})\n";
            return evaluation.ExecuteAsync(script);
        }

        public static Task NextPlotAsync(this IRExpressionEvaluator evaluation, Guid deviceId)
        {
            Debug.Assert(deviceId != Guid.Empty);
            var script = $"rtvs:::graphics.ide.nextplot({deviceId.ToString().ToRStringLiteral()})\n";
            return evaluation.ExecuteAsync(script);
        }

        public static Task PreviousPlotAsync(this IRExpressionEvaluator evaluation, Guid deviceId)
        {
            Debug.Assert(deviceId != Guid.Empty);
            var script = $"rtvs:::graphics.ide.previousplot({deviceId.ToString().ToRStringLiteral()})\n";
            return evaluation.ExecuteAsync(script);
        }

        public static Task ClearPlotHistoryAsync(this IRExpressionEvaluator evaluation, Guid deviceId)
        {
            Debug.Assert(deviceId != Guid.Empty);
            var script = $"rtvs:::graphics.ide.clearplots({deviceId.ToString().ToRStringLiteral()})\n";
            return evaluation.ExecuteAsync(script);
        }

        public static Task RemoveCurrentPlotAsync(this IRExpressionEvaluator evaluation, Guid deviceId, Guid plotId)
        {
            Debug.Assert(deviceId != Guid.Empty);
            Debug.Assert(plotId != Guid.Empty);
            var script = $"rtvs:::graphics.ide.removeplot({deviceId.ToString().ToRStringLiteral()}, {plotId.ToString().ToRStringLiteral()})\n";
            return evaluation.ExecuteAsync(script);
        }

        public static Task CopyPlotAsync(this IRExpressionEvaluator evaluation, Guid sourceDeviceId, Guid sourcePlotId, Guid targetDeviceId)
        {
            Debug.Assert(sourceDeviceId != Guid.Empty);
            Debug.Assert(sourcePlotId != Guid.Empty);
            Debug.Assert(targetDeviceId != Guid.Empty);
            var script = $"rtvs:::graphics.ide.copyplot({sourceDeviceId.ToString().ToRStringLiteral()}, {sourcePlotId.ToString().ToRStringLiteral()}, {targetDeviceId.ToString().ToRStringLiteral()})\n";
            return evaluation.ExecuteAsync(script);
        }

        public static Task SelectPlotAsync(this IRExpressionEvaluator evaluation, Guid deviceId, Guid plotId)
        {
            Debug.Assert(deviceId != Guid.Empty);
            Debug.Assert(plotId != Guid.Empty);
            var script = $"rtvs:::graphics.ide.selectplot({deviceId.ToString().ToRStringLiteral()}, {plotId.ToString().ToRStringLiteral()})\n";
            return evaluation.ExecuteAsync(script);
        }

        public static async Task ActivatePlotDeviceAsync(this IRExpressionEvaluator evaluation, Guid deviceId)
        {
            Debug.Assert(deviceId != Guid.Empty);
            await evaluation.ExecuteAsync($"rtvs:::graphics.ide.setactivedeviceid({deviceId.ToString().ToRStringLiteral()})");
        }

        public static Task<int?> GetPlotDeviceNumAsync(this IRExpressionEvaluator evaluation, Guid deviceId)
        {
            Debug.Assert(deviceId != Guid.Empty);
            return evaluation.EvaluateAsync<int?>($"rtvs:::graphics.ide.getdevicenum({deviceId.ToString().ToRStringLiteral()})", REvaluationKind.Normal);
        }

        public static async Task NewPlotDeviceAsync(this IRExpressionEvaluator evaluation)
        {
            await evaluation.ExecuteAsync($"ide()");
        }

        public static async Task<Guid> GetActivePlotDeviceAsync(this IRExpressionEvaluator evaluation)
        {
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
            var script = @"rtvs:::packages.installed()";
            return evaluation.EvaluateAsync<JArray>(script, REvaluationKind.Normal);
        }

        public static Task<JArray> AvailablePackagesAsync(this IRExpressionEvaluator evaluation) {
            var script = @"rtvs:::packages.available()";
            return evaluation.EvaluateAsync<JArray>(script, REvaluationKind.Reentrant);
        }

        public static Task<JArray> LoadedPackagesAsync(this IRExpressionEvaluator evaluation) {
            var script = @"rtvs:::packages.loaded()";
            return evaluation.EvaluateAsync<JArray>(script, REvaluationKind.Normal);
        }

        public static Task<JArray> LibraryPathsAsync(this IRExpressionEvaluator evaluation) {
            var script = @"rtvs:::packages.libpaths()";
            return evaluation.EvaluateAsync<JArray>(script, REvaluationKind.Normal);
        }

        public static Task ExportPlotToBitmapAsync(this IRExpressionEvaluator evaluation, Guid deviceId, Guid plotId, string deviceName, string outputFilePath, int widthInPixels, int heightInPixels, int resolution)
        {
            string script = $"rtvs:::graphics.ide.exportimage({deviceId.ToString().ToRStringLiteral()}, {plotId.ToString().ToRStringLiteral()}, {outputFilePath.ToRPath().ToRStringLiteral()}, {deviceName}, {widthInPixels}, {heightInPixels}, {resolution})";
            return evaluation.ExecuteAsync(script);
        }

        public static Task ExportPlotToMetafileAsync(this IRExpressionEvaluator evaluation, Guid deviceId, Guid plotId, string outputFilePath, double widthInInches, double heightInInches, int resolution)
        {
            string script = $"rtvs:::graphics.ide.exportimage({deviceId.ToString().ToRStringLiteral()}, {plotId.ToString().ToRStringLiteral()}, {outputFilePath.ToRPath().ToRStringLiteral()}, win.metafile, {widthInInches}, {heightInInches}, {resolution})";
            return evaluation.ExecuteAsync(script, REvaluationKind.Normal);
        }

        public static Task ExportToPdfAsync(this IRExpressionEvaluator evaluation, Guid deviceId, Guid plotId, string outputFilePath, double widthInInches, double heightInInches)
        {
            string script = $"rtvs:::graphics.ide.exportpdf({deviceId.ToString().ToRStringLiteral()}, {plotId.ToString().ToRStringLiteral()}, {outputFilePath.ToRPath().ToRStringLiteral()}, {widthInInches}, {heightInInches})";
            return evaluation.ExecuteAsync(script, REvaluationKind.Normal);
        }

        public static async Task SetVsCranSelectionAsync(this IRExpressionEvaluator evaluation, string mirrorUrl) {
            await evaluation.ExecuteAsync(Invariant($"rtvs:::set_mirror({mirrorUrl.ToRStringLiteral()})"));
        }

        public static Task SetROptionsAsync(this IRExpressionEvaluator evaluation) {
            var script =
@"options(help_type = 'html')
  options(browser = rtvs:::open_url)
  options(pager = rtvs:::show_file)
";
            return evaluation.ExecuteAsync(script);
        }

        public static Task SetCodePageAsync(this IRExpressionEvaluator evaluation, int codePage) {
            if (codePage == 0) {
                codePage = NativeMethods.GetOEMCP();
            }
            var script = Invariant($"Sys.setlocale('LC_CTYPE', '.{codePage}')");
            return evaluation.ExecuteAsync(script);
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
    }
}