using System.Collections.Generic;

namespace Microsoft.R.Editor.Completion.Definitions
{
    /// <summary>
    /// An interface implemented by R completion provider that supplies
    /// list of entries to intellisense. There may be more than one provider.
    /// Providers are exported via MEF.
    /// </summary>
    public interface IRCompletionListProvider
    {
        /// <summary>
        /// Retrieves list of intellisense entries
        /// </summary>
        /// <param name="context">Completion context</param>
        /// <returns>List of completion entries</returns>
        IList<RCompletion> GetEntries(RCompletionContext context);

        /// <summary>
        /// Completion type this provider is registered for
        /// </summary>
        string CompletionType { get; }
    }
}
