using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Repl.Session
{
    public static class RSessionInteractionCommands
    {
        public static async Task Quit(this IRSessionInteraction interaction)
        {
            await interaction.RespondAsync("q()\n");
        }

        public static async Task OptionsSetWidth(this IRSessionInteraction interaction, int width)
        {
            await interaction.RespondAsync(Invariant($"options(width=as.integer({width}))\n"));
        }

        public static async Task SetWorkingDirectory(this IRSessionInteraction interaction, string path)
        {
            await interaction.RespondAsync(Invariant($"setwd('{path.Replace('\\', '/')}')\n"));
        }

        public static async Task SetDefaultWorkingDirectory(this IRSessionInteraction interaction)
        {
            await interaction.RespondAsync(Invariant($"setwd('~')\n"));
        }
    }
}