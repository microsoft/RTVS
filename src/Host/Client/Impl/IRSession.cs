using System;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client
{
    public interface IRSession : IDisposable
    {
        event EventHandler<RBeforeRequestEventArgs> BeforeRequest;
        event EventHandler<RResponseEventArgs> Response;
        event EventHandler<RErrorEventArgs> Error;

        string Prompt { get; }

        Task<IRSessionInteraction> BeginInteractionAsync(bool isVisible = true);
        Task InitializeAsync();
    }
}