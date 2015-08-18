using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help.Functions
{
    /// <summary>
    /// Contains index of function to package improving 
    /// performance of locating package that contains 
    /// the function documentation.
    /// </summary>
    public static partial class FunctionIndex
    {
        private static Dictionary<string, string> _functionToPackageMap;
        private static Dictionary<string, string> _functionToDescriptionMap;
        private static Dictionary<string, IReadOnlyList<string>> _functionToSignaturesMap;
        private static Dictionary<string, IReadOnlyList<string>> _packageToFunctionsMap;

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
        public static IReadOnlyList<string> GetPackageFunctions(string packageName)
        {
            if (_packageToFunctionsMap != null)
            {
                IReadOnlyList<string> packageFunctions;
                if (_packageToFunctionsMap.TryGetValue(packageName, out packageFunctions))
                {
                    return packageFunctions;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves list of functions in a given package
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
        /// Retrieves list of functions in a given package
        /// </summary>
        public static IReadOnlyList<string> GetFunctionSignatureStrings(string functionName)
        {
            if (_functionToSignaturesMap != null)
            {
                IReadOnlyList<string> signatures;
                if (_functionToSignaturesMap.TryGetValue(functionName, out signatures))
                {
                    return signatures;
                }
            }

            return null;
        }

        public static void AddFunctionDescription(INamedItemInfo functionInfo)
        {
            if (_functionToDescriptionMap == null)
            {
                _functionToDescriptionMap = new Dictionary<string, string>();
            }

            _functionToDescriptionMap[functionInfo.Name] = functionInfo.Description;
        }

        public static void AddFunctionData(IFunctionInfo functionInfo)
        {
            AddFunctionDescription(functionInfo);
            AddFunctionToPackage(functionInfo);
            AddFunctionSignatures(functionInfo);
        }

        public static void AddPackageData(IPackageInfo packageInfo)
        {
            foreach (IFunctionInfo function in packageInfo.Functions)
            {
                AddFunctionToPackage(function);
            }
        }

        private static void AddFunctionToPackage(IFunctionInfo functionInfo)
        {
            List<string> functions = null;
            if (_packageToFunctionsMap == null)
            {
                _packageToFunctionsMap = new Dictionary<string, IReadOnlyList<string>>();
                functions = new List<string>();
                _packageToFunctionsMap[functionInfo.PackageName] = functions;
            }
            else
            {
                IReadOnlyList<string> funcs;
                if (_packageToFunctionsMap.TryGetValue(functionInfo.Name, out funcs))
                {
                    functions = funcs as List<string>;
                }
            }

            Debug.Assert(functions != null);
            if (functions != null && !functions.Contains(functionInfo.Name))
            {
                functions.Add(functionInfo.Name);
            }
        }

        private static void AddFunctionSignatures(IFunctionInfo functionInfo)
        {
            List<string> signatures = null;
            if (_functionToSignaturesMap == null)
            {
                _functionToSignaturesMap = new Dictionary<string, IReadOnlyList<string>>();
                signatures = new List<string>();
                _functionToSignaturesMap[functionInfo.Name] = signatures;
            }
            else
            {
                IReadOnlyList<string> sigs;
                if (_functionToSignaturesMap.TryGetValue(functionInfo.Name, out sigs))
                {
                    signatures = sigs as List<string>;
                }
            }

            Debug.Assert(signatures != null);
            foreach (var signature in functionInfo.Signatures)
            {
                string sigString = signature.GetSignatureString(functionInfo.Name);
                signatures.Add(sigString);
            }
        }
    }
}
