// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Design;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.DataInspect.Commands;
using Microsoft.VisualStudio.R.Package.Debugger.Commands;
using Microsoft.VisualStudio.R.Package.Documentation;
using Microsoft.VisualStudio.R.Package.Feedback;
using Microsoft.VisualStudio.R.Package.Help;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Options.R.Tools;
using Microsoft.VisualStudio.R.Package.Plots.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Repl.Data;
using Microsoft.VisualStudio.R.Package.Repl.Debugger;
using Microsoft.VisualStudio.R.Package.Repl.Workspace;
using Microsoft.VisualStudio.R.Package.RPackages.Commands;

namespace Microsoft.VisualStudio.R.Packages.R {
    internal static class PackageCommands {
        public static IEnumerable<MenuCommand> GetCommands(ExportProvider exportProvider) {
            var interactiveWorkflowProvider = exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
            var interactiveWorkflowComponentContainerFactory = exportProvider.GetExportedValue<IInteractiveWindowComponentContainerFactory>();
            var interactiveWorkflow = interactiveWorkflowProvider.GetOrCreate();
            var projectServiceAccessor = exportProvider.GetExportedValue<IProjectServiceAccessor>();
            var plotHistoryProvider = exportProvider.GetExportedValue<IPlotHistoryProvider>();
            var plotHistory = plotHistoryProvider.GetPlotHistory(interactiveWorkflow.RSession);
            var textViewTracker = exportProvider.GetExportedValue<IActiveWpfTextViewTracker>();
            var replTracker = exportProvider.GetExportedValue<IActiveRInteractiveWindowTracker>();
            var debuggerModeTracker = exportProvider.GetExportedValue<IDebuggerModeTracker>();

            return new List<MenuCommand> {
                new GoToOptionsCommand(),
                new GoToEditorOptionsCommand(),
                new ImportRSettingsCommand(),

                new SendSmileCommand(),
                new SendFrownCommand(),

                new OpenDocumentationCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRtvsDocumentation, DocumentationUrls.RtvsDocumentation),
                new OpenDocumentationCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRtvsSamples, DocumentationUrls.RtvsSamples),
                new OpenDocumentationCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRDocsIntroToR, DocumentationUrls.CranIntro),
                new OpenDocumentationCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRDocsTaskViews, DocumentationUrls.CranViews),
                new OpenDocumentationCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRDocsDataImportExport, DocumentationUrls.CranData),
                new OpenDocumentationCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdRDocsWritingRExtensions, DocumentationUrls.CranExtensions),
                new OpenDocumentationCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdCheckForUpdates, DocumentationUrls.CheckForRtvsUpdates),
                new OpenDocumentationCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdMicrosoftRProducts, DocumentationUrls.MicrosoftRProducts),

                new LoadWorkspaceCommand(interactiveWorkflow, projectServiceAccessor),
                new SaveWorkspaceCommand(interactiveWorkflow, projectServiceAccessor),

                new AttachDebuggerCommand(interactiveWorkflow),
                new AttachToRInteractiveCommand(interactiveWorkflow),
                new StopDebuggingCommand(interactiveWorkflow),
                new ContinueDebuggingCommand(interactiveWorkflow),
                new StepOverCommand(interactiveWorkflow),
                new StepOutCommand(interactiveWorkflow),
                new StepIntoCommand(interactiveWorkflow),
                new SourceRScriptCommand(interactiveWorkflow, textViewTracker),

                new InterruptRCommand(interactiveWorkflow, debuggerModeTracker),
                new ResetReplCommand(interactiveWorkflow),

                new ImportDataSetTextFileCommand(),
                new ImportDataSetUrlCommand(),

                new InstallPackagesCommand(),
                new CheckForPackageUpdatesCommand(),

                // Window commands
                new ShowPlotWindowsCommand(),
                new ShowRInteractiveWindowsCommand(interactiveWorkflowProvider, interactiveWorkflowComponentContainerFactory),
                new ShowVariableWindowCommand(),
                new ShowHelpWindowCommand(),
                new ShowHelpOnCurrentCommand(interactiveWorkflow, textViewTracker, replTracker),
                new ShowHistoryWindowCommand(),

                // Plot commands
                new ExportPlotAsImageCommand(plotHistory),
                new ExportPlotAsPdfCommand(plotHistory),
                new CopyPlotAsBitmapCommand(plotHistory),
                new CopyPlotAsMetafileCommand(plotHistory),
                new HistoryNextPlotCommand(plotHistory),
                new HistoryPreviousPlotCommand(plotHistory),
                new ClearPlotsCommand(plotHistory),
                new RemovePlotCommand(plotHistory),
            };
        }
    }
}
