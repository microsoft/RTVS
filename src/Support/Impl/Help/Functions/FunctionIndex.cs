using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.R.Support.Engine;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help.Functions
{
    /// <summary>
    /// Provides information on functions in packages for intellisense.
    /// Since loading list of functions requires parsing of HTML index
    /// files in packages help, it caches information and persists
    /// cache to disk.
    /// </summary>
    public static partial class FunctionIndex
    {
        private static ConcurrentDictionary<string, BlockingCollection<string>> _packageToFunctionsMap = new ConcurrentDictionary<string, BlockingCollection<string>>();
        private static ConcurrentDictionary<string, string> _functionToPackageMap = new ConcurrentDictionary<string, string>();
        private static ConcurrentDictionary<string, string> _functionToDescriptionMap = new ConcurrentDictionary<string, string>();
        private static ConcurrentDictionary<string, IFunctionInfo> _functionToInfoMap = new ConcurrentDictionary<string, IFunctionInfo>();

        private static RdFunctionHelp _rdFunctionHelp;

        public static void Initialize()
        {
            if (_rdFunctionHelp == null)
            {
                _rdFunctionHelp = new RdFunctionHelp();
            }
        }

        /// <summary>
        /// Given function name provides name of the containing package
        /// </summary>
        public static string GetFunctionPackage(string functionName)
        {
            if (_functionToPackageMap != null)
            {
                string packageName;
                if (_functionToPackageMap.TryGetValue(functionName, out packageName))
                {
                    return packageName;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Retrieves list of functions in a given package
        /// </summary>
        public static IReadOnlyCollection<string> GetPackageFunctions(string packageName)
        {
            if (_packageToFunctionsMap != null)
            {
                BlockingCollection<string> packageFunctions;
                if (_packageToFunctionsMap.TryGetValue(packageName, out packageFunctions))
                {
                    return packageFunctions;
                }
            }

            return new List<string>();
        }

        /// <summary>
        /// Retrieves function description
        /// </summary>
        public static string GetFunctionDescription(string functionName)
        {
            if (_functionToDescriptionMap != null)
            {
                string description;
                if (_functionToDescriptionMap.TryGetValue(functionName, out description))
                {
                    return description;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Retrieves function information by name
        /// </summary>
        public static IFunctionInfo GetFunctionInfo(string functionName)
        {
            if (_functionToInfoMap != null)
            {
                IFunctionInfo functionInfo;
                if (_functionToInfoMap.TryGetValue(functionName, out functionInfo))
                {
                    return functionInfo;
                }
                else
                {
                    string packageName;
                    if (_functionToPackageMap.TryGetValue(functionName, out packageName))
                    {
                        GetFunctionInfoFromEngineAsync(functionName, packageName);
                    }
                    else
                    {
                        Debug.Assert(false, "Function without package: " + functionName);
                    }
                }
            }

            return null;
        }

        private static Task GetFunctionInfoFromEngineAsync(string functionName, string packageName)
        {
            return Task.Run(async () =>
            {
                EngineResponse response = await _rdFunctionHelp.GetFunctionRdHelp(functionName, packageName, OnFunctionInfoReady);
            });
        }

        private static void OnFunctionInfoReady(object obj)
        {
            IFunctionInfo functionInfo = obj as IFunctionInfo;
            if(functionInfo != null)
            {
                _functionToInfoMap[functionInfo.Name] = functionInfo;
            }
        }
    }
}
