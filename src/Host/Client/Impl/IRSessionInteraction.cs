using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client
{
    public interface IRSessionInteraction
    {
        string Prompt { get; }
        int MaxLength { get; }
        IReadOnlyCollection<IRContext> Contexts { get; }
        Task<string> RespondAsync(string messageText);
    }
}