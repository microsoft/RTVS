using System.Collections.Generic;
using Microsoft.R.Editor.Completion.Definitions;

namespace Microsoft.R.Editor.Completion.Provider
{
    /// <summary>
    /// A default completion provider that does nothing (i.e. returns an empty 
    /// list of intellisen entries). Used when no other providers are found 
    /// for a given context.
    /// </summary>
    public class DefaultCompletionProvider : IRCompletionListProvider
    {
        #region IRCompletionListProvider
        public string CompletionType
        {
            get { return CompletionTypes.None; }
        }

        public IList<RCompletion> GetEntries(RCompletionContext context)
        {
            return new List<RCompletion>();
        }
        #endregion
    }
}
