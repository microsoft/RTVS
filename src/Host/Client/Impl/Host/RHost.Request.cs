using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Tasks;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Host.Client {
    partial class RHost {
        /// <summary>
        /// A pending request to the host process that is awaiting response.
        /// </summary>
        private abstract class Request {
            public readonly ulong Id;
            public readonly string MessageName;
            public readonly JArray Json;

            protected Request(RHost host, Message message) {
                Id = message.Id;
                MessageName = message.Name;
                Json = message.Json;

                host._requests[Id] = this;
            }

            public abstract void Handle(RHost host, Message response);
        }

        private abstract class Request<T> : Request {
            protected readonly TaskCompletionSource<T> CompletionSource = new TaskCompletionSource<T>();

            public Task<T> Task => CompletionSource.Task;

            protected Request(RHost host, Message message, CancellationToken cancellationToken) : base(host, message) {
                if (cancellationToken.CanBeCanceled) {
                    CompletionSource
                        .RegisterForCancellation(cancellationToken)
                        .UnregisterOnCompletion(CompletionSource.Task);
                }
            }
        }
    }
}