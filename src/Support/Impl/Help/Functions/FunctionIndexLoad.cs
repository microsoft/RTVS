using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Shell;

namespace Microsoft.R.Support.Help.Functions
{
    /// <summary>
    /// Contains index of function to package improving 
    /// performance of locating package that contains 
    /// the function documentation.
    /// </summary>
    public static partial class FunctionIndex
    {
        private static Task _indexReadingTask;

        /// <summary>
        /// Returns path to the RTVS data cache location
        /// </summary>
        public static string RtvsDataPath
        {
            get
            {
                // Index is stored under regular R location: 
                // ~/Documents/R in the RTVS folder
                string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return Path.Combine(myDocuments, "R", "RTVS");
            }
        }

        public static string IndexFilePath
        {
            get { return Path.Combine(RtvsDataPath, "FunctionIndex.idx"); }
        }

        public static void LoadIndexAsync()
        {
            if (_indexReadingTask == null)
            {
                _indexReadingTask = Task.Run(() => LoadIndexAsync());
            }
        }

        public static void CompleteLoad()
        {
            if(_indexReadingTask != null)
            {
                _indexReadingTask.Wait();
            }
        }

        private static void LoadIndexAsync()
        {
            Dictionary<string, string> functionToPackageMap = new Dictionary<string, string>();
            Dictionary<string, string> functionToDescriptionMap = new Dictionary<string, string>();
            Dictionary<string, IReadOnlyList<string>> functionToSignaturesMap = new Dictionary<string, IReadOnlyList<string>>();
            Dictionary<string, IReadOnlyList<string>> packageToFunctionsMap = new Dictionary<string, IReadOnlyList<string>>();
            bool loaded = false;

            // ~/Documents/R/RTVS/FunctionsIndex.dx -> function to package map, also contains function description
            // ~/Documents/R/RTVS/[PackageName]/[FunctionName].sig -> function signatures
            try
            {
                using (StreamReader sr = new StreamReader(IndexFilePath))
                {
                    // Each line is a pair of function name followed by package name
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
                            functionToDescriptionMap[functionName] = functionDescription;

                            IReadOnlyList<string> functions;
                            if (!packageToFunctionsMap.TryGetValue(packageName, out functions))
                            {
                                functions = new List<string>();
                                packageToFunctionsMap[packageName] = functions;
                            }

                            IList<string> list = functions as IList<string>;
                            list.Add(functionName);
                        }
                    }
                }
            }
            catch (IOException) { }

            if (!loaded)
            {
                return;
            }

            try
            {
                foreach (string functionName in functionToPackageMap.Keys)
                {
                    string packageName = functionToPackageMap[functionName];
                    string packageFolderName = Path.Combine(RtvsDataPath, packageName);
                    string signaturesFileName = Path.Combine(packageFolderName, functionName + ".sig");

                    using (StreamReader sr = new StreamReader(signaturesFileName))
                    {
                        while (true)
                        {
                            string line = sr.ReadLine();
                            if (line == null)
                            {
                                break;
                            }

                            IReadOnlyList<string> signatures;
                            if (!packageToFunctionsMap.TryGetValue(packageName, out signatures))
                            {
                                signatures = new List<string>();
                                functionToSignaturesMap[functionName] = signatures;
                            }

                            IList<string> list = signatures as IList<string>;
                            list.Add(line);
                        }
                    }
                }
            }
            catch (IOException)
            {
                loaded = false;
            }

            if (loaded)
            {
                EditorShell.DispatchOnUIThread(() =>
                {
                    _functionToPackageMap = functionToPackageMap;
                    _functionToDescriptionMap = functionToDescriptionMap;
                    _functionToSignaturesMap = functionToSignaturesMap;
                    _packageToFunctionsMap = packageToFunctionsMap;
                });
            }
        }
    }
}
