// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;

namespace Microsoft.R.Editor.Functions {
    public interface IFunctionIndex {
        /// <summary>
        /// Provides access to services in extension methods
        /// </summary>
        IServiceContainer Services { get; }

        /// <summary>
        /// Builds function index
        /// </summary>
        /// <param name="packageIndex">
        /// Package index, if available. If not available, 
        /// index builder will attempt to obtain it from the service container
        /// </param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Awaitable task</returns>
        Task BuildIndexAsync(IPackageIndex packageIndex = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Registers all functions from the package in the index
        /// </summary>
        /// <param name="package">PAckage information</param>
        void RegisterPackageFunctions(IPackageInfo package);

        /// <summary>
        /// Attempts to retrieve cached information.
        /// </summary>
        /// <param name="functionName">Function name</param>
        /// <param name="packageName">Package name. Can be null - in some cases index 
        /// can determine the package name. Alternatively, invoke <see cref="GetPackageNameAsync"/> 
        /// first.</param>
        /// <returns>Function information or null if not found.</returns>
        IFunctionInfo GetFunctionInfo(string functionName, string packageName = null);

        /// <summary>
        /// Attempts to locate and cache function information. When it completes
        /// it is possible to call <see cref="GetFunctionInfo"/> 
        /// right away and get the function information.
        /// </summary>
        /// <param name="functionName">Function name</param>
        /// <param name="packageName">Package name</param>
        /// <returns></returns>
        Task<IFunctionInfo> GetFunctionInfoAsync(string functionName, string packageName = null);

        /// <summary>
        /// Attempts to determine package the function belongs to. Package name depends 
        /// on the order of loading. For example, 'select' may be from 'MASS' or from 'dplyr' 
        /// depending which package was loaded last. The function also retrieves and caches 
        /// function information so it is possible to call <see cref="GetFunctionInfo"/> 
        /// right away and get the function information.
        /// </summary>
        /// <param name="functionName">Name of the function</param>
        /// <returns>Name of the package</returns>
        Task<string> GetPackageNameAsync(string functionName);
    }
}