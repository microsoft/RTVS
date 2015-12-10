using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Host.Client.Session {
    public static class RSessionInteractionCommands {
        public static Task Quit(this IRSessionInteraction interaction) {
            return interaction.RespondAsync("q()\n");
        }
    }
}