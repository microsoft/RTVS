using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.R.Actions.Logging;

namespace Microsoft.R.Actions.Script
{
    /// <summary>
    /// Implements installation of R packages with dependencies
    /// </summary>
    public static class InstallPackages
    {
        /// <summary>
        /// Asynchronously installs a set of R packages with dependencies
        /// </summary>
        public static RCommand Install(IEnumerable<string> packageNames, IActionLog log)
        {
            string arguments = PackageListToString(packageNames);
            return Install(arguments, log);
        }

        /// <summary>
        /// Asynchronously install one R packages with dependencies
        /// </summary>
        public static RCommand Install(string packageName, IActionLog log)
        {
            return RCommand.ExecuteAsync("INSTALL " + packageName, log);
        }

        /// <summary>
        /// Synchronously install a set of R packages with dependencies.
        /// Typically only used during setup from the MSI custom action.
        /// </summary>
        public static void InstallSynchronously(IEnumerable<string> packageNames, int msTimeout, IActionLog log)
        {
            string arguments = PackageListToString(packageNames);
            if (!Install(arguments, log).Task.Wait(msTimeout))
            {
                log.WriteFormatAsync(MessageCategory.Error, Resources.Error_InstallTimeout_Format, arguments);
            }
        }

        public static bool IsInstalled(string packageName, int msTimeout)
        {
            string expression = "installed.packages()";
            IActionLinesLog log = new LinesLog();

            bool result = RCommand.ExecuteRExpression(expression, log, msTimeout);
            if (result)
            {
                // stdout is list of packages
                // abind "abind" "C:/Users/[USER_NAME]/Documents/R/win-library/3.2" "1.4-3"   NA
                return FindPackageName(packageName, log.Lines);
            }

            return false;
        }

        private static string PackageListToString(IEnumerable<string> packageNames)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string packageName in packageNames)
            {
                sb.Append(packageName);
                sb.Append(' ');
            }

            return sb.ToString();
        }

        private static bool FindPackageName(string packageName, IReadOnlyCollection<string> lines)
        {
            // abind "abind" "C:/Users/[...]/Documents/R/win-library/3.2" "1.4-3" NA
            foreach (string s in lines)
            {
                if (s.Length >= packageName.Length + 1)
                {
                    if(s.StartsWith(packageName, StringComparison.OrdinalIgnoreCase) && char.IsWhiteSpace(s[packageName.Length]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
