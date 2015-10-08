using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Repl.Session {
    public static class RSessionInteractionCommands {
        public static async Task Quit(this IRSessionInteraction interaction) {
            await interaction.RespondAsync("q()\n");
        }
    }
}