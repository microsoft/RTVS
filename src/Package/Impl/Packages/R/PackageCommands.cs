using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Design;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.DataInspect.Commands;
using Microsoft.VisualStudio.R.Package.Feedback;
using Microsoft.VisualStudio.R.Package.Help;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Options.R.Tools;
using Microsoft.VisualStudio.R.Package.Plots.Commands;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Repl.Data;
using Microsoft.VisualStudio.R.Package.Repl.Debugger;
using Microsoft.VisualStudio.R.Package.Repl.Workspace;
using Microsoft.VisualStudio.R.Package.RPackages.Commands;

namespace Microsoft.VisualStudio.R.Packages.R {
    internal static class PackageCommands {
        public static IEnumerable<MenuCommand> GetCommands(ExportProvider exportProvider) {
            var interactiveWorkflowProvider = exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
            var interactiveWorkflow = interactiveWorkflowProvider.GetOrCreate();
            var projectServiceAccessor = exportProvider.GetExportedValue<IProjectServiceAccessor>();

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

                new InterruptRCommand(interactiveWorkflow),

                new ImportDataSetTextFileCommand(),
                new ImportDataSetUrlCommand(),

                new InstallPackagesCommand(),
                new CheckForPackageUpdatesCommand(),

                // Window commands
                new ShowPlotWindowsCommand(),
                new ShowRInteractiveWindowsCommand(interactiveWorkflowProvider),
                new ShowVariableWindowCommand(),
                new ShowHelpWindowCommand(),
                new ShowHelpOnCurrentCommand(),
                new ShowHistoryWindowCommand(),

                // Plot commands
                new ExportPlotAsImageCommand(),
                new ExportPlotAsPdfCommand(),
                new CopyPlotAsBitmapCommand(),
                new CopyPlotAsMetafileCommand(),
                new HistoryNextPlotCommand(),
                new HistoryPreviousPlotCommand()
            };
        }
    }
}
