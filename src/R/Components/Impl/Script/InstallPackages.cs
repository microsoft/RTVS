// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Common.Core.Logging;

namespace Microsoft.R.Components.Script {
    /// <summary>
    /// Implements installation of R packages with dependencies
    /// </summary>
    public static class InstallPackages {
        /// <summary>
        /// Asynchronously installs a set of R packages with dependencies
        /// </summary>
        public static RCommand Install(string rBasePath, IEnumerable<string> packageNames, IActionLog log) {
            string arguments = PackageListToString(packageNames);
            return Install(arguments, log, rBasePath);
        }

        /// <summary>
        /// Asynchronously install one R packages with dependencies
        /// </summary>
        public static RCommand Install(string packageName, IActionLog log, string rBasePath) {
            return RCommand.ExecuteAsync("INSTALL " + packageName, log, rBasePath);
        }

        /// <summary>
        /// Synchronously install a set of R packages with dependencies.
        /// Typically only used during setup from the MSI custom action.
        /// </summary>
        public static void InstallSynchronously(IEnumerable<string> packageNames, int msTimeout, IActionLog log, string rBasePath) {
            string arguments = PackageListToString(packageNames);
            if (!Install(arguments, log, rBasePath).Task.Wait(msTimeout)) {
                log.WriteFormatAsync(MessageCategory.Error, Resources.Error_InstallTimeout_Format, arguments);
            }
        }

        private static string PackageListToString(IEnumerable<string> packageNames) {
            StringBuilder sb = new StringBuilder();

            foreach (string packageName in packageNames) {
                sb.Append(packageName);
                sb.Append(' ');
            }

            return sb.ToString();
        }
    }
}
