using System;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRSession : IDisposable {
        event EventHandler<RRequestEventArgs> BeforeRequest;
        event EventHandler<RRequestEventArgs> AfterRequest;
        event EventHandler<EventArgs> Mutated;
        event EventHandler<ROutputEventArgs> Output;
        event EventHandler<EventArgs> Connected;
        event EventHandler<EventArgs> Disconnected;
        event EventHandler<EventArgs> Disposed;

        int Id { get; }
        string Prompt { get; }
        bool IsHostRunning { get; }

        Task<IRSessionInteraction> BeginInteractionAsync(bool isVisible = true);
        Task<IRSessionEvaluation> BeginEvaluationAsync(bool isMutating = true);
        Task CancelAllAsync();
        Task StartHostAsync();
        Task StopHostAsync();

        void FlushLog();
    }
}