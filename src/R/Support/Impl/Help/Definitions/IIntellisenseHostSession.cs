// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Support.Help {
    public interface IIntellisenseRSession : IDisposable {
        /// <summary>
        /// Intellisense R session. The session is different from the interactive
        /// session and is used to build indexes and to retrieve RD data for
        /// function descriptions and signatures.
        /// </summary>
        IRSession Session { get; }

        /// <summary>
        /// Starts intellisense session.
        /// </summary>
        Task StartSessionAsync();
        
        /// <summary>
        /// Given function name returns package the function belongs to.
        /// The package is determined from the interactive R session since
        /// there may be functions with the same name but from different packages.
        /// Most recently loaded package typically wins.
        /// </summary>
        /// <param name="functionName">R function name</param>
        /// <returns>Function package or null if undefined</returns>
        Task<string> GetFunctionPackageNameAsync(string functionName);

        /// <summary>
        /// Retrieves names of packages loaded into the interactive session.
        /// </summary>
        IEnumerable<string> LoadedPackageNames { get; }
    }
}
