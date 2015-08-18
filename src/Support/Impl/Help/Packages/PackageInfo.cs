using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Html.Core.Tree;
using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.Utility;

namespace Microsoft.R.Support.Help.Packages
{
    public sealed class PackageInfo : AsyncDataSource<string>, IPackageInfo
    {
        #region IPackageInfo
        public string Name { get; private set; }

        public string InstallPath { get; private set; }

        public bool IsBase { get; internal set; }

        public string Description
        {
            get
            {
                if (_description == null)
                {
                    Task.Run(() =>
                    {
                        _description = GetDescription();
                        SetData(_description);
                    });
                }

                return _description ?? string.Empty;
            }
        }

        public IReadOnlyCollection<INamedItemInfo> Functions
        {
            get
            {
                return Func
                IReadOnlyList<string> functions = FunctionIndex.GetPackageFunctions(this.Name);
                if (functions == null)
                {
                    functions = GetFunctions();
                }

                return _functions.Values;
            }
        }
        #endregion

        private string _description;
        private PackageFunctionsInfo _functionsInfo;

        public PackageInfo(string name, string installPath)
        {
            Name = name;
            InstallPath = installPath;

            _functionsInfo = new PackageFunctionsInfo(name, )
            this.DataReady += OnDataReady;
        }

        private void OnDataReady(object sender, string e)
        {
            FunctionIndex.AddPackageData
        }

        /// <summary>
        /// Extract package description from the DESCRITION file on disk
        /// </summary>
        private string GetDescription()
        {
            StringBuilder sb = new StringBuilder();
            bool found = false;

            try
            {
                // DESCRIPTION uses a simple file format called DCF, the Debian control format. 
                // Each line consists of a field name and a value, separated by a colon.
                // When values span multiple lines, they need to be indented:
                //
                // Description: The description of a package is usually long,
                //    spanning multiple lines. The second and subsequent lines
                //    should be indented, usually with four spaces.

                string descriptionFilePath = Path.Combine(InstallPath, Name, "DESCRIPTION");

                using (StreamReader sr = new StreamReader(descriptionFilePath, Encoding.UTF8))
                {
                    while (!found)
                    {
                        string line = sr.ReadLine();
                        if (line == null)
                        {
                            break;
                        }

                        if (line.StartsWith("Description:", StringComparison.OrdinalIgnoreCase))
                        {
                            line = line.Substring(12).Trim();
                            sb.Append(line);
                            sb.Append(' ');

                            while (!found)
                            {
                                line = sr.ReadLine();
                                if (line == null)
                                {
                                    break;
                                }

                                if (line.Length > 0 && char.IsWhiteSpace(line[0]))
                                {
                                    line.Trim();

                                    sb.Append(line);
                                    sb.Append(' ');
                                }
                                else
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (IOException) { }

            return sb.ToString().Trim();
        }
    }
}
