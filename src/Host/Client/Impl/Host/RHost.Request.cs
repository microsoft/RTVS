using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Tasks;
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
            protected readonly TaskCompletionSourceEx<T> CompletionSource = new TaskCompletionSourceEx<T>();

            public Task<T> Task => CompletionSource.Task;

            protected Request(RHost host, Message message, CancellationToken cancellationToken)
                : base(host, message) {
                cancellationToken.Register(() => CompletionSource.TrySetCanceled(null, cancellationToken));
            }
        }
    }
}