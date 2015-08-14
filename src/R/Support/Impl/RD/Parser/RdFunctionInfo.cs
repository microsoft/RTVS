using System.Collections.Generic;

namespace Microsoft.R.Support.RD.Parser
{
    public sealed class RdFunctionInfo
    {
        /// <summary>
        /// Function name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Other function name variants
        /// </summary>
        public IReadOnlyCollection<string> Aliases { get; internal set; }

        public bool IsInternal { get; internal set; }

        /// <summary>
        /// Function description
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Function sugnatures
        /// </summary>
        public IReadOnlyCollection<SignatureInfo> Signatures { get; internal set; }

        /// <summary>
        /// Return value description
        /// </summary>
        public string ReturnValue { get; internal set; }

        internal bool IsComplete
        {
            get { return Description != null && Signatures != null && ReturnValue != null; }
        }

        public RdFunctionInfo(string name)
        {
            Name = name;
        }
    }
}
