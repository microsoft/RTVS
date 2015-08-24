using System.Collections.Generic;

namespace Microsoft.R.Support.Help.Definitions
{
    public interface ISignatureInfo
    {
        /// <summary>
        /// Function arguments
        /// </summary>
        IReadOnlyList<IArgumentInfo> Arguments { get; }

        /// <summary>
        /// Creates formatted signature that is presented to the user
        /// during function parameter completion. Optionally provides
        /// locus points (locations withing the string) for each function
        /// parameter.
        /// </summary>
        string GetSignatureString(string functionName, List<int> locusPoints = null);
    }
}
