using System;

namespace Microsoft.R.Host.Client {
    public class REvaluationEventArgs : EventArgs {
        public bool IsMutating { get; }

        public REvaluationEventArgs(bool isMutating) {
            IsMutating = isMutating;
        }
    }
}