using System;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client
{
    public interface IRSession : IDisposable
    {
        event EventHandler<RBeforeRequestEventArgs> BeforeRequest;
        event EventHandler<RResponseEventArgs> Response;
        event EventHandler<RErrorEventArgs> Error;
        event EventHandler<EventArgs> Disconnected;

        string Prompt { get; }
        bool IsHostRunning { get; }

        Task<IRSessionInteraction> BeginInteractionAsync(bool isVisible = true);
        Task<IRSessionEvaluation> BeginEvaluationAsync();
        Task StartHostAsync();
        Task StopHostAsync();
    }
}