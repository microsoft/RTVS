using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Design;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.DataInspect.Commands;
using Microsoft.VisualStudio.R.Package.Feedback;
using Microsoft.VisualStudio.R.Package.Help;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Options.R.Tools;
using Microsoft.VisualStudio.R.Package.Plots.Commands;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Repl.Data;
using Microsoft.VisualStudio.R.Package.Repl.Debugger;
using Microsoft.VisualStudio.R.Package.Repl.Workspace;
using Microsoft.VisualStudio.R.Package.RPackages.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Packages.R {
    internal static class PackageCommands {
        public static IEnumerable<MenuCommand> GetCommands(ExportProvider exportProvider) {
            var interactiveWorkflowProvider = exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
            var interactiveWorkflow = interactiveWorkflowProvider.GetOrCreate();
            var projectServiceAccessor = exportProvider.GetExportedValue<IProjectServiceAccessor>();
            var plotHistory = exportProvider.GetExportedValue<IPlotHistory>();
            var debugger = VsAppShell.Current.GetGlobalService<IVsDebugger>(typeof(IVsDebugger));
            var textViewTracker = exportProvider.GetExportedValue<IActiveWpfTextViewTracker>();
            var debuggerModeTracker = exportProvider.GetExportedValue<IDebuggerModeTracker>();

            return new List<MenuCommand> {
                new GoToOptionsCommand(),
                new GoToEditorOptionsCommand(),
                new ImportRSettingsCommand(),

                new SendSmileCommand(),
                new SendFrownCommand(),

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
                new ShowRInteractiveWindowsCommand(interactiveWorkflowProvider),
                new ShowVariableWindowCommand(),
                new ShowHelpWindowCommand(),
                new ShowHelpOnCurrentCommand(interactiveWorkflow.RSession, textViewTracker),
                new ShowHistoryWindowCommand(),

                // Plot commands
                new ExportPlotAsImageCommand(plotHistory),
                new ExportPlotAsPdfCommand(plotHistory),
                new CopyPlotAsBitmapCommand(plotHistory),
                new CopyPlotAsMetafileCommand(plotHistory),
                new HistoryNextPlotCommand(plotHistory),
                new HistoryPreviousPlotCommand(plotHistory)
            };
        }
    }
}
