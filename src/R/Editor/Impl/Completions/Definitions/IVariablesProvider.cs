// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.R.Editor.Functions;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// Provides information on variables members or
    /// variables declared in a global scope.
    /// </summary>
    public interface IVariablesProvider {
        void Initialize();

        /// <summary>
        /// Given variable name determines number of members
        /// </summary>
        /// <param name="variableName">Variable name or null if global scope</param>
        int GetMemberCount(string variableName);

        /// <summary>
        /// Given variable name returns variable members
        /// adhering to specified criteria. Last member name
        /// may be partial such as abc$def$g
        /// </summary>
        /// <param name="variableName">
        /// Variable name such as abc$def$g. 'g' may be partially typed
        /// in which case providers returns members of 'def' filtered to 'g' prefix.
        /// </param>
        /// <param name="maxCount">Max number of members to return</param>
        IReadOnlyCollection<INamedItemInfo> GetMembers(string variableName, int maxCount);
    }
}
