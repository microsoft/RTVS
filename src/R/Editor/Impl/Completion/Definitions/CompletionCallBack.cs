using System;

namespace Microsoft.R.Editor.Completion.Definitions {
    internal sealed class CompletionCallBack<T> {
        public Action<T, object> Action;
        public object Parameter;
        public RCompletionContext Context;
    }
}
