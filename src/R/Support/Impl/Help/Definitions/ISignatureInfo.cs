using System.Collections.Generic;

namespace Microsoft.R.Support.Help.Definitions {
    public interface ISignatureInfo {
        /// <summary>
        /// Function name
        /// </summary>
        string FunctionName { get; }
        /// <summary>
        /// Function arguments
        /// </summary>
        IList<IArgumentInfo> Arguments { get; }

        /// <summary>
        /// Creates formatted signature that is presented to the user
        /// during function parameter completion. Optionally provides
        /// locus points (locations withing the string) for each function
        /// parameter.
        /// </summary>
        string GetSignatureString(List<int> locusPoints = null);

        /// <summary>
        /// Given argument name returns index of the argument in the signature.
        /// Performs full and then partial matching of the argument name.
        /// </summary>
        /// <param name="argumentName">Name of the argument</param>
        /// <param name="partialMatch">
        /// If true, partial match will be performed 
        /// if exact match is not found
        /// </param>
        /// <returns>Argument index or -1 if argumen is not named or was not found</returns>
        int GetArgumentIndex(string argumentName, bool partialMatch);
    }
}
