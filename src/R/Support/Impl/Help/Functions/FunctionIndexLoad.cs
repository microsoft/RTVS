// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if FUNCTION_INDEX_CACHE
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.RD.Parser;

namespace Microsoft.R.Support.Help.Functions
{
    /// <summary>
    /// Contains index of function to package improving 
    /// performance of locating package that contains 
    /// the function documentation.
    /// </summary>
    public static partial class FunctionIndex
    {
        /// <summary>
        /// Returns path to the RTVS data cache location
        /// </summary>
        private static string RtvsDataPath
        {
            get
            {
                // Index is stored under regular R location: 
                // ~/Documents/R in the RTVS folder
                string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return Path.Combine(myDocuments, "R", "RTVS");
            }
        }

        private static string IndexFilePath
        {
            get { return Path.Combine(RtvsDataPath, "FunctionIndex.idx"); }
        }

        private static bool LoadIndex()
        {
            ConcurrentDictionary<string, string> functionToPackageMap = new ConcurrentDictionary<string, string>();
            ConcurrentDictionary<string, IFunctionInfo> functionToInfoMap = new ConcurrentDictionary<string, IFunctionInfo>();
            ConcurrentDictionary<string, BlockingCollection<INamedItemInfo>> packageToFunctionsMap = new ConcurrentDictionary<string, BlockingCollection<INamedItemInfo>>();
            bool loaded = false;

            // ~/Documents/R/RTVS/FunctionsIndex.dx -> function to package map, also contains function description
            // ~/Documents/R/RTVS/[PackageName]/[FunctionName].sig -> function signatures

            // Function index format:
            //      Each line is a triplet of function name followed 
            //      by the package name followed by the function description
            //      There are no tabs in the function description.

            try
            {
                if (File.Exists(IndexFilePath))
                {
                    using (StreamReader sr = new StreamReader(IndexFilePath))
                    {
                        char[] separator = new char[] { '\t' };

                        while (true)
                        {
                            string line = sr.ReadLine();
                            if (line == null)
                            {
                                break;
                            }

                            string[] parts = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length == 3)
                            {
                                string functionName = parts[0];
                                string packageName = parts[1];
                                string functionDescription = parts[2];

                                if (functionName == "completed" && packageName == "completed" && functionDescription == "completed")
                                {
                                    loaded = true;
                                    break;
                                }

                                functionToPackageMap[functionName] = packageName;

                                BlockingCollection<INamedItemInfo> functions;
                                if (!packageToFunctionsMap.TryGetValue(packageName, out functions))
                                {
                                    functions = new BlockingCollection<INamedItemInfo>();
                                    packageToFunctionsMap[packageName] = functions;
                                }

                                functions.Add(new NamedItemInfo(functionName, functionDescription));
                            }
                        }
                    }
                }
            }
            catch (IOException) { }

            if (!loaded)
            {
                return false;
            }

            if (LoadFunctions(functionToPackageMap))
            {
                EditorShell.DispatchOnUIThread(() =>
                {
                    _functionToPackageMap = functionToPackageMap;
                    _packageToFunctionsMap = packageToFunctionsMap;
                    _functionToInfoMap = functionToInfoMap;
                });
            }

            return loaded;
        }

        private static bool LoadFunctions(ConcurrentDictionary<string, string> functionToPackageMap)
        {
            try
            {
                // Function data format:
                //      1 if function is internal, 0 otherwise
                //      Function description (one long line)
                //      Function signatures (one per line)
                //      an empty line
                //      argument descriptions (one per line) in a form 
                //          argument_name: description

                foreach (string functionName in functionToPackageMap.Keys)
                {
                    string packageName = functionToPackageMap[functionName];
                    string packageFolderName = Path.Combine(RtvsDataPath, packageName);
                    string signaturesFileName = Path.Combine(packageFolderName, functionName + ".fd");

                    using (StreamReader sr = new StreamReader(signaturesFileName))
                    {
                        bool isInternal = ReadInternal(sr);
                        string description = sr.ReadLine().Trim();
                        List<string> signatureStrings = ReadSignatures(sr);
                        Dictionary<string, string> arguments = ReadArguments(sr);

                        List<ISignatureInfo> signatureInfos = new List<ISignatureInfo>();
                        foreach (string s in signatureStrings)
                        {
                            SignatureInfo info = RdFunctionSignature.ParseSignature(s);
                            signatureInfos.Add(info);
                        }

                        foreach (SignatureInfo sig in signatureInfos)
                        {
                            foreach(ArgumentInfo arg in sig.Arguments)
                            {
                                string argDescription;
                                if (arguments.TryGetValue(arg.Name, out argDescription))
                                {
                                    arg.Description = argDescription;
                                }
                            }
                        }

                        FunctionInfo functionInfo = new FunctionInfo(functionName, description);
                        functionInfo.IsInternal = isInternal;
                        functionInfo.Signatures = signatureInfos;
                    }
                }
            }
            catch (IOException)
            {
                return false;
            }

            return true;
        }

        private static List<string> ReadSignatures(StreamReader sr)
        {
            List<string> signatures = new List<string>();

            while (true)
            {
                string line = sr.ReadLine().Trim();
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }

                signatures.Add(line);
            }

            return signatures;
        }

        private static Dictionary<string, string> ReadArguments(StreamReader sr)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();

            while (true)
            {
                string line = sr.ReadLine().Trim();
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }

                int index = line.IndexOf(':');
                if (index < 0)
                    throw new IOException();

                string name = line.Substring(0, index);
                string description = line.Substring(index + 1);

                arguments[name] = description;
            }

            return arguments;
        }

        private static bool ReadInternal(StreamReader sr)
        {
            string line = sr.ReadLine().Trim();

            int value;
            if (!Int32.TryParse(line, out value))
            {
                throw new IOException();
            }

            return value > 0;
        }
    }
}
#endif
