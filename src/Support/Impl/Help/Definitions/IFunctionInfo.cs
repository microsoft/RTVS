using System;
using System.Collections.Generic;

namespace Microsoft.R.Support.Help.Definitions
{
    public interface IFunctionInfo: INamedItemInfo
    {
        /// <summary>
        /// Package the function comes from
        /// </summary>
        string PackageName { get; }

        /// <summary>
        /// Other function name variants
        /// </summary>
        IReadOnlyList<string> Aliases { get; }

        /// <summary>
        /// Function sugnatures
        /// </summary>
        IReadOnlyList<SignatureInfo> Signatures { get; }

        /// <summary>
        /// Return value description
        /// </summary>
        string ReturnValue { get; }

        /// <summary>
        /// Indicates that function is internal (has 'internal' 
        /// in its list of keywords)
        /// </summary>
        bool IsInternal { get; }
    }
}
