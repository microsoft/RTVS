using System.Collections.Generic;

namespace Microsoft.R.Support.RD.Parser
{
    public sealed class SignatureInfo
    {
        /// <summary>
        /// Function arguments
        /// </summary>
        public IReadOnlyCollection<ArgumentInfo> Arguments { get; internal set; }
     }
}
