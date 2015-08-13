using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Tree;
using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Support.Utility;

namespace Microsoft.R.Support.Packages
{
    public sealed class PackageInfo : AsyncDataSource<string>
    {
        public string Name { get; private set; }

        public string InstallPath { get; private set; }

        public string Description
        {
            get
            {
                if (_description == null && !_fetchDescription)
                {
                    _description = GetDescription();
                }

                return _description;
            }
        }

        public IEnumerable<FunctionInfo> Functions
        {
            get
            {
                if (_functions == null)
                {
                    if (_functionsLoadTask != null)
                    {
                        _functionsLoadTask.Wait();
                        _functionsLoadTask = null;
                    }
                    else
                    {
                        GetFunctions();
                    }
                }

                return _functions;
            }
        }

        private string _description;
        private List<FunctionInfo> _functions;
        private bool _fetchDescription;
        private Task _functionsLoadTask;

        public PackageInfo(string name) :
            this(name, InstalledPackages.UserPackagesPath, false)
        {
        }

        public PackageInfo(string name, string installPath, bool fetchDescription = false)
        {
            Name = name;
            InstallPath = installPath;
            _fetchDescription = fetchDescription;

            if (fetchDescription)
            {
                Task.Run(() =>
                {
                    _description = GetDescription();
                    SetData(_description);
                });
            }
        }

        public Task LoadFunctionsAsync()
        {
            if (_functions == null && _functionsLoadTask == null)
            {
                _functionsLoadTask = new Task(() => GetFunctions());
                _functionsLoadTask.Start();
                return _functionsLoadTask;
            }

            return Task.FromResult<object>(null);
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

        private void GetFunctions()
        {
            string content = null;

            try
            {
                string htmlFile = Path.Combine(InstallPath, Name, "html", "00index.html");

                using (StreamReader sr = new StreamReader(htmlFile, Encoding.UTF8))
                {
                    content = sr.ReadToEnd();
                }
            }
            catch (IOException) { }

            if (!string.IsNullOrEmpty(content))
            {
                HtmlTree tree = new HtmlTree(new Microsoft.Web.Core.Text.TextStream(content));
                tree.Build();

                List<FunctionInfo> functions = new List<FunctionInfo>();
                FunctionSearch functionSearch = new FunctionSearch(functions);

                tree.Accept(functionSearch, null);
                _functions = functions;
            }
        }

        private class FunctionSearch : IHtmlTreeVisitor
        {
            private List<FunctionInfo> _functions;

            public FunctionSearch(List<FunctionInfo> functions)
            {
                _functions = functions;
            }

            public bool Visit(ElementNode element, object parameter)
            {
                if (element.Name.Equals("tr", StringComparison.OrdinalIgnoreCase) && element.Children.Count == 2)
                {
                    var functionInfo = new FunctionInfo();

                    ElementNode tdNode1 = element.Children[0];
                    ElementNode tdNode2 = element.Children[1];

                    if (tdNode1.Children.Count == 1 && tdNode1.Children[0].Name.Equals("a", StringComparison.OrdinalIgnoreCase))
                    {
                        functionInfo.Name = element.Root.TextProvider.GetText(tdNode1.Children[0].InnerRange);
                        if (IsValidName(functionInfo.Name))
                        {
                            functionInfo.Description = element.Root.TextProvider.GetText(tdNode2.InnerRange);
                            if (functionInfo.Name != null && functionInfo.Description != null)
                            {
                                _functions.Add(functionInfo);
                            }
                        }
                    }
                }

                return true;
            }
            private bool IsValidName(string name)
            {
                bool hasCharacters = false;

                if (name.Length == 0 || !char.IsLetter(name[0]))
                {
                    return false;
                }

                for (int i = 0; i < name.Length; i++)
                {
                    if(char.IsLetterOrDigit(name[i]))
                    {
                        hasCharacters = true;
                    }
                    else if (name[i] != '.' && name[i] != '_')
                    {
                        return false;
                    }
                }

                return hasCharacters;
            }
        }
    }
}
