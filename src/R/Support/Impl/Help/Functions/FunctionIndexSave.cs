// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if FUNCTION_INDEX_CACHE

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
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
        private static Task _indexWritingTask;

        public static Task SaveIndexAsync()
        {
            if (_indexWritingTask == null)
            {
                _indexWritingTask = Task.Run(() => SaveIndex());
                return _indexWritingTask;
            }

            return Task.FromResult<object>(null);
        }

        public static void CompleteSave()
        {
            if (_indexWritingTask != null)
            {
                _indexWritingTask.Wait();
            }
        }

        private static void SaveIndex()
        {
            // ~/Documents/R/RTVS/FunctionsIndex.idx -> function to package map, also contains function description
            // ~/Documents/R/RTVS/[PackageName]/[FunctionName].fd -> function data

            // Function index format:
            //      Each line is a triplet of function name followed 
            //      by the package name followed by the function description
            //      There are no tabs in the function description.

            try
            {
                using (StreamWriter sw = new StreamWriter(IndexFilePath))
                {
                    foreach (string packageName in _packageToFunctionsMap.Keys)
                    {
                        BlockingCollection<INamedItemInfo> functions;
                        _packageToFunctionsMap.TryGetValue(packageName, out functions);

                        if (functions == null)
                            throw new IOException();

                        foreach (INamedItemInfo function in functions)
                        {
                            string line = string.Format(CultureInfo.InvariantCulture, "{0}\t{1}\t{2}", function.Name, packageName, function.Description ?? string.Empty);
                            sw.WriteLine(line);
                        }

                        sw.WriteLine("completed completed completed");
                    }
                }
            }
            catch (IOException)
            {
                return;
            }

            SaveFunctions();
        }

        private static void SaveFunctions()
        {
            // Function data format:
            //      1 if function is internal, 0 otherwise
            //      Function description (one long line)
            //      Function signatures (one per line)
            //      an empty line
            //      argument descriptions (one per line) in a form 
            //          argument_name: description

            try
            {
                foreach (string functionName in _functionToPackageMap.Keys)
                {
                    IFunctionInfo functionInfo;
                    _functionToInfoMap.TryGetValue(functionName, out functionInfo);

                    if (functionInfo != null)
                    {
                        string packageName = null;
                        _functionToPackageMap.TryGetValue(functionName, out packageName);

                        string packageFolderName = Path.Combine(RtvsDataPath, packageName);
                        if (!Directory.Exists(packageFolderName))
                        {
                            Directory.CreateDirectory(packageFolderName);
                        }

                        string signaturesFileName = Path.Combine(packageFolderName, functionName + ".fd");
                        Dictionary<string, string> arguments = new Dictionary<string, string>();

                        using (StreamWriter sw = new StreamWriter(signaturesFileName))
                        {
                            sw.WriteLine(functionInfo.IsInternal ? "1" : "0");
                            sw.WriteLine(functionInfo.Description);

                            foreach (ISignatureInfo signatureInfo in functionInfo.Signatures)
                            {
                                string s = signatureInfo.GetSignatureString(functionName);
                                sw.WriteLine(s);

                                foreach (IArgumentInfo arg in signatureInfo.Arguments)
                                {
                                    if (!string.IsNullOrEmpty(arg.Description) && !arguments.ContainsKey(arg.Name))
                                    {
                                        arguments[arg.Name] = arg.Description;
                                    }
                                }
                            }

                            sw.WriteLine(string.Empty);

                            foreach (string argName in arguments.Keys)
                            {
                                sw.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}:{1}", argName, arguments[argName]));
                            }
                        }
                    }
                }
            }
            catch (IOException) { }
        }
    }
}
#endif
