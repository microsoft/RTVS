using System;
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
        /// <summary>
        /// Maps package name to a list of functions in the package.
        /// Used to extract function names and descriptions when
        /// showing list of functions available in the file.
        /// </summary>
        private static ConcurrentDictionary<string, BlockingCollection<INamedItemInfo>> _packageToFunctionsMap = new ConcurrentDictionary<string, BlockingCollection<INamedItemInfo>>();

        /// <summary>
        /// Map of functions to packages. Used to quickly find package 
        /// name by function name as we need both to get the function 
        /// documentation in RD format from the R engine.
        /// </summary>
        private static ConcurrentDictionary<string, string> _functionToPackageMap = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Map of function names to complete function information.
        /// Used to construct and show quick info tooltips as well
        /// as help and completion of the function signature.
        /// </summary>
        private static ConcurrentDictionary<string, IFunctionInfo> _functionToInfoMap = new ConcurrentDictionary<string, IFunctionInfo>();

        /// <summary>
        /// R engine session that extracts functions RD documentation
        /// by function and package names.
        /// </summary>
        private static RdFunctionHelp _rdFunctionHelp;

        /// <summary>
        /// Initialized function index and starts R engine session
        /// that is used to get RD documentation on functions.
        /// </summary>
        public static void Initialize()
        {
            if (_rdFunctionHelp == null)
            {
                _rdFunctionHelp = new RdFunctionHelp();
            }
        }

        public static void Terminate()
        {
            if (_rdFunctionHelp != null)
            {
                _rdFunctionHelp.Dispose();
                _rdFunctionHelp = null;
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
        public static IReadOnlyCollection<INamedItemInfo> GetPackageFunctions(string packageName)
        {
            if (_packageToFunctionsMap != null)
            {
                BlockingCollection<INamedItemInfo> packageFunctions;
                if (_packageToFunctionsMap.TryGetValue(packageName, out packageFunctions))
                {
                    return packageFunctions;
                }
            }

            return new List<INamedItemInfo>();
        }

        /// <summary>
        /// Retrieves function information by name
        /// </summary>
        public static IFunctionInfo GetFunctionInfo(string functionName,
                                  Action<object> infoReadyCallback = null, object parameter = null)
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
                        GetFunctionInfoFromEngineAsync(functionName, packageName, infoReadyCallback, parameter);
                    }
                    else
                    {
                        //Debug.Assert(false, "Function without package: " + functionName);
                    }
                }
            }

            return null;
        }

        private static void GetFunctionInfoFromEngineAsync(string functionName, string packageName,
                                         Action<object> infoReadyCallback = null, object parameter = null)
        {
            _rdFunctionHelp.GetFunctionRdHelp(
                functionName,
                packageName,
                (object o) =>
                {
                    if (o != null)
                    {
                        OnFunctionInfoReady(o);

                        if (infoReadyCallback != null)
                        {
                            infoReadyCallback(parameter);
                        }
                    }
                });
        }

        private static void OnFunctionInfoReady(object obj)
        {
            IFunctionInfo functionInfo = obj as IFunctionInfo;
            if (functionInfo != null)
            {
                if (functionInfo.Aliases != null)
                {
                    foreach (string alias in functionInfo.Aliases)
                    {
                        _functionToInfoMap[alias] = functionInfo;
                    }
                }
                else if (!string.IsNullOrEmpty(functionInfo.Name))
                {
                    _functionToInfoMap[functionInfo.Name] = functionInfo;
                }
            }
        }
    }
}
