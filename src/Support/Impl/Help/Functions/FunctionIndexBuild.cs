using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Packages;

namespace Microsoft.R.Support.Help.Functions
{
    /// <summary>
    /// Contains index of function to package improving 
    /// performance of locating package that contains 
    /// the function documentation.
    /// </summary>
    public static partial class FunctionIndex
    {
        private static Task _indexBuildingTask;

        public static Task BuildIndexAsync()
        {
            if (_indexBuildingTask == null)
            {
                _indexBuildingTask = Task.Run(() =>
                {
                    DateTime startIndexLoad = DateTime.Now;
                    bool result = LoadIndex();
                    if (result)
                    {
                        Debug.WriteLine("R functions index load time: {0} ms", (DateTime.Now - startIndexLoad).TotalMilliseconds);
                    }
                    else
                    {
                        DateTime startIndexBuild = DateTime.Now;
                        BuildIndex();
                        Debug.WriteLine("R functions index build time: {0} ms", (DateTime.Now - startIndexBuild).TotalMilliseconds);
                    }
                });

                return _indexBuildingTask;
            }

            return Task.FromResult<object>(null);
        }

        public static void CompleteBuild()
        {
            if (_indexBuildingTask != null)
            {
                _indexBuildingTask.Wait();
            }
        }

        private static void BuildIndex()
        {
            IReadOnlyList<IPackageInfo> packages = PackageIndex.Packages;
            foreach (IPackageInfo p in packages)
            {
                foreach (INamedItemInfo f in p.Functions)
                {
                    AddFunctionToPackage(p.Name, f);
                }
            }
        }


        private static void AddFunctionToPackage(string packageName, INamedItemInfo function)
        {
            BlockingCollection<INamedItemInfo> funcs;

            if (!_packageToFunctionsMap.TryGetValue(packageName, out funcs))
            {
                funcs = new BlockingCollection<INamedItemInfo>();
                _packageToFunctionsMap[packageName] = funcs;
            }

            _functionToPackageMap[function.Name] = packageName;
            funcs.Add(function);
        }
    }
}
