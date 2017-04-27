// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Editor.Functions {
    public interface ISignatureInfo {
        /// <summary>
        /// Primary function name (the name that the signature is specified for)
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
        /// <param name="actualName">
        /// Actual function name (may be alias different from <see cref="FunctionName"/> but with the same signature)
        /// </param>
        /// <param name="locusPoints">Locations of arguments inside the formatted signature</param>
        string GetSignatureString(string actualName, List<int> locusPoints = null);

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
