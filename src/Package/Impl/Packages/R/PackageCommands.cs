using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.R.Package.Commands.Global;
using Microsoft.VisualStudio.R.Package.Plots.Commands;

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

            return commands;
        }
    }
}
