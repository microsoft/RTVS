using System;
using System.Collections.Generic;

namespace Microsoft.R.Editor.Completion.Definitions
{
    /// <summary>
    /// An interface implemented by R completion provider that supplies
    /// list of entries to intellisense asynchronously.
    /// </summary>
    public interface IRAsyncCompletionListProvider
    {
        /// <summary>
        /// Retrieves list of intellisense entries asynchronously.
        /// Calls back the supplied function when the list is ready.
        /// </summary>
        void GetEntriesAsync(RCompletionContext context, 
            Action<IReadOnlyCollection<RCompletion>, object> callback, object callbackParameter);
    }
}
