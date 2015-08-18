using System.Collections.Generic;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Packages;

namespace Microsoft.R.Support.Help.Functions
{
    public sealed class FunctionInfo: NamedItemInfo
    {
        /// <summary>
        /// Package the function comes from
        /// </summary>
        public string PackageName { get; internal set; }

        /// <summary>
        /// Other function name variants
        /// </summary>
        public IReadOnlyList<string> Aliases { get; internal set; }

        /// <summary>
        /// Function sugnatures
        /// </summary>
        public IReadOnlyList<ISignatureInfo> Signatures { get; internal set; }

        /// <summary>
        /// Return value description
        /// </summary>
        public string ReturnValue { get; internal set; }

        /// <summary>
        /// Indicates that function is internal (has 'internal' 
        /// in its list of keywords)
        /// </summary>
        public bool IsInternal { get; internal set; }

        internal bool IsComplete
        {
            get { return Description != null && Signatures != null && ReturnValue != null; }
        }

        public FunctionInfo(string name)
        {
            Name = name;
        }
    }
}
