using System.Collections.Generic;
using System.ComponentModel.Design;
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

            commands.Add(new InstallPackagesCommand());
            commands.Add(new CheckForPackageUpdatesCommand());

            commands.Add(new ShowPlotWindowsCommand());
            commands.Add(new ShowRInteractiveWindowsCommand());
 
            return commands;
        }
    }
}
