// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager.Commands;
using Microsoft.R.Components.Documentation;
using Microsoft.R.Components.Documentation.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Commands;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.R.Components.Plots.Commands;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.Sql;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.DataInspect.Commands;
using Microsoft.VisualStudio.R.Package.Feedback;
using Microsoft.VisualStudio.R.Package.Help;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Options.R.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Repl.Debugger;
using Microsoft.VisualStudio.R.Package.Repl.Shiny;
using Microsoft.VisualStudio.R.Package.Repl.Workspace;
using Microsoft.VisualStudio.R.Package.Sql;
using Microsoft.VisualStudio.R.Package.ToolWindows;
using Microsoft.VisualStudio.R.Package.Windows;
using Microsoft.VisualStudio.Utilities;
using IServiceContainer = Microsoft.Common.Core.Services.IServiceContainer;
using static Microsoft.VisualStudio.R.Package.Commands.CommandAsyncToOleMenuCommandShimFactory;

namespace Microsoft.VisualStudio.R.Packages.R {
    internal static class PackageCommands {
        public static IEnumerable<MenuCommand> GetCommands(IServiceContainer services) {
            var interactiveWorkflowProvider = services.GetService<IRInteractiveWorkflowVisualProvider>();
            var interactiveWorkflow = interactiveWorkflowProvider.GetOrCreate();
            var projectServiceAccessor = services.GetService<IProjectServiceAccessor>();
            var textViewTracker = services.GetService<IActiveWpfTextViewTracker>();
            var replTracker = services.GetService<IActiveRInteractiveWindowTracker>();
            var debuggerModeTracker = services.GetService<IDebuggerModeTracker>();
            var pss = services.GetService<IProjectSystemServices>();
            var pcsp = services.GetService<IProjectConfigurationSettingsProvider>();
            var dbcs = services.GetService<IDbConnectionService>();
            var settings = services.GetService<IRSettings>();
            var ui = services.UI();
            var shell = services.GetService<ICoreShell>();
            var console = new InteractiveWindowConsole(interactiveWorkflow);

            return new List<MenuCommand> {
                new GoToOptionsCommand(services),
                new GoToEditorOptionsCommand(services),
                new ImportRSettingsCommand(services),
                new InstallRClientCommand(services),
                new SurveyNewsCommand(services),
                new SetupRemoteCommand(),

                new ReportIssueCommand(services),
                new SendSmileCommand(services),
                new SendFrownCommand(services),

                CreateRCmdSetCommand(RPackageCommandId.icmdRDocsIntroToR, new OpenDocumentationCommand(interactiveWorkflow, OnlineDocumentationUrls.CranIntro, LocalDocumentationPaths.CranIntro)),
                CreateRCmdSetCommand(RPackageCommandId.icmdRDocsDataImportExport, new OpenDocumentationCommand(interactiveWorkflow, OnlineDocumentationUrls.CranData, LocalDocumentationPaths.CranData)),
                CreateRCmdSetCommand(RPackageCommandId.icmdRDocsWritingRExtensions, new OpenDocumentationCommand(interactiveWorkflow, OnlineDocumentationUrls.CranExtensions, LocalDocumentationPaths.CranExtensions)),
                CreateRCmdSetCommand(RPackageCommandId.icmdRDocsTaskViews, new OpenDocumentationCommand(interactiveWorkflow, OnlineDocumentationUrls.CranViews)),
                CreateRCmdSetCommand(RPackageCommandId.icmdRtvsDocumentation, new OpenDocumentationCommand(interactiveWorkflow, OnlineDocumentationUrls.RtvsDocumentation)),
                CreateRCmdSetCommand(RPackageCommandId.icmdRtvsSamples, new OpenDocumentationCommand(interactiveWorkflow, OnlineDocumentationUrls.RtvsSamples)),
                CreateRCmdSetCommand(RPackageCommandId.icmdCheckForUpdates, new OpenDocumentationCommand(interactiveWorkflow, OnlineDocumentationUrls.CheckForRtvsUpdates)),
                CreateRCmdSetCommand(RPackageCommandId.icmdMicrosoftRProducts, new OpenDocumentationCommand(interactiveWorkflow, OnlineDocumentationUrls.MicrosoftRProducts)),

                new LoadWorkspaceCommand(shell, interactiveWorkflow, projectServiceAccessor),
                new SaveWorkspaceCommand(shell, interactiveWorkflow, projectServiceAccessor),

                new AttachDebuggerCommand(interactiveWorkflow),
                new AttachToRInteractiveCommand(interactiveWorkflow),
                new StopDebuggingCommand(interactiveWorkflow),
                new ContinueDebuggingCommand(interactiveWorkflow),
                new StepOverCommand(interactiveWorkflow),
                new StepOutCommand(interactiveWorkflow),
                new StepIntoCommand(interactiveWorkflow),

                CreateRCmdSetCommand(RPackageCommandId.icmdSourceRScript, new SourceRScriptCommand(interactiveWorkflow, textViewTracker, false)),
                CreateRCmdSetCommand(RPackageCommandId.icmdSourceRScriptWithEcho, new SourceRScriptCommand(interactiveWorkflow, textViewTracker, true)),

                new RunShinyAppCommand(interactiveWorkflow),
                new StopShinyAppCommand(interactiveWorkflow),

                CreateRCmdSetCommand(RPackageCommandId.icmdInterruptR, new InterruptRCommand(interactiveWorkflow, debuggerModeTracker)),
                CreateRCmdSetCommand(RPackageCommandId.icmdTerminateR, new TerminateRCommand(interactiveWorkflow, ui)),
                CreateRCmdSetCommand(RPackageCommandId.icmdSessionInformation, new SessionInformationCommand(interactiveWorkflow, console)),
                CreateRCmdSetCommand(RPackageCommandId.icmdDeleteProfile, new DeleteProfileCommand(interactiveWorkflow)),

                new ResetReplCommand(interactiveWorkflow),
                new PrevHistoryReplCommand(interactiveWorkflow),
                new NextHistoryReplCommand(interactiveWorkflow),

                // Directory management
                new SetDirectoryToSourceCommand(interactiveWorkflow, textViewTracker),
                new SetDirectoryToProjectCommand(interactiveWorkflow, pss),
                new SelectWorkingDirectoryCommand(interactiveWorkflow),

                new ImportDataSetTextFileCommand(services, interactiveWorkflow.RSession),
                new ImportDataSetUrlCommand(services, interactiveWorkflow.RSession),
                new DeleteAllVariablesCommand(interactiveWorkflow),
                new AddDbConnectionCommand(dbcs, pss, pcsp, interactiveWorkflow),
                new AddDsnCommand(shell, interactiveWorkflow),
                new ManageDsnCommand(shell, interactiveWorkflow),

                // Window commands
                new ShowRInteractiveWindowsCommand(interactiveWorkflowProvider),
                new ShowVariableWindowCommand(),

                new ShowToolWindowCommand<HelpWindowPane>(RPackageCommandId.icmdShowHelpWindow),
                new ShowToolWindowCommand<HistoryWindowPane>(RPackageCommandId.icmdShowHistoryWindow),
                new ShowToolWindowCommand<ConnectionManagerWindowPane>(RPackageCommandId.icmdShowConnectionsWindow),
                new ShowToolWindowCommand<PackageManagerWindowPane>(RPackageCommandId.icmdShowPackagesWindow),
                new ShowToolWindowCommand<PlotHistoryWindowPane>(RPackageCommandId.icmdPlotHistoryWindow),

                new ShowHelpOnCurrentCommand(interactiveWorkflow, textViewTracker, replTracker),
                new SearchWebForCurrentCommand(interactiveWorkflow, textViewTracker, replTracker),
                new GotoEditorWindowCommand(textViewTracker, services),
                new GotoSolutionExplorerCommand(shell),

                // Plot commands
                CreateRCmdSetCommand(RPackageCommandId.icmdNewPlotWindow, new PlotDeviceNewCommand(interactiveWorkflow)),
                CreateRCmdSetCommand(RPackageCommandId.icmdShowPlotWindow, new ShowMainPlotWindowCommand(interactiveWorkflow)),
                CreateRCmdSetCommand(RPackageCommandId.icmdPlotWindowsDynamicStart, new ShowPlotWindowCommand(shell, interactiveWorkflow)),
                new HideAllPlotWindowsCommand(shell),

                // Connection manager commands
                CreateRCmdSetCommand(RPackageCommandId.icmdReconnect, new ReconnectCommand(interactiveWorkflow)),
                CreateRCmdSetCommand(RPackageCommandId.icmdMruConnectionsDynamicStart, new SwitchToConnectionCommand(interactiveWorkflow, settings))
            };
        }
    }
}
