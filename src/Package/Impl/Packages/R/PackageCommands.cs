using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Package.DataInspect.Commands;
using Microsoft.VisualStudio.R.Package.Options.R.Tools;
using Microsoft.VisualStudio.R.Package.Plots.Commands;
using Microsoft.VisualStudio.R.Package.Repl.Data;
using Microsoft.VisualStudio.R.Package.Repl.Workspace;
using Microsoft.VisualStudio.R.Package.RPackages.Commands;

namespace Microsoft.VisualStudio.R.Packages.R
{
    internal static class PackageCommands
    {
        public static IEnumerable<MenuCommand> GetCommands()
        {
            var commands = new List<MenuCommand>();

            commands.Add(new GoToOptionsCommand());

            commands.Add(new LoadWorkspaceCommand());
            commands.Add(new SaveWorkspaceCommand());
            commands.Add(new RestartRCommand());
            commands.Add(new InterruptRCommand());

            commands.Add(new ImportDataSetTextFileCommand());
            commands.Add(new ImportDataSetUrlCommand());

            commands.Add(new SavePlotCommand());
            commands.Add(new ExportPlotCommand());
            commands.Add(new FixPlotCommand());
            commands.Add(new CopyPlotCommand());
            commands.Add(new PrintPlotCommand());
            commands.Add(new ZoomInPlotCommand());
            commands.Add(new ZoomOutPlotCommand());

            commands.Add(new InstallPackagesCommand());
            commands.Add(new CheckForPackageUpdatesCommand());

            commands.Add(new ShowPlotWindowsCommand());
            commands.Add(new ShowRInteractiveWindowsCommand());

            commands.Add(new ShowVariableWindowCommand());

            return commands;
        }
    }
}
