using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Repl.Session
{
    public static class RSessionInteractionCommands
    {
        public static async Task Quit(this IRSessionInteraction session)
        {
            await session.RespondAsync("q()\n");
        }
    }
}