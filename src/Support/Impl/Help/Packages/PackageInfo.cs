using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Html.Core.Tree;
using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;

namespace Microsoft.R.Support.Help.Packages
{
    public sealed class PackageInfo : NamedItemInfo, IPackageInfo
    {
        private string _description;

        public PackageInfo(string name, string installPath): 
            base(name)
        {
            InstallPath = installPath;
        }

        #region NamedItemInfo
        public override string Description
        {
            get
            {
                if (_description == null)
                    _description = GetDescription();

                return _description;
            }
        }
        #endregion

        #region IPackageInfo
        /// <summary>
        /// Package install path
        /// </summary>
        public string InstallPath { get; private set; }

        /// <summary>
        /// Collection of functions in the package
        /// </summary>
        public IReadOnlyCollection<INamedItemInfo> Functions
        {
            get { return FunctionIndex.GetPackageFunctions(this.Name); }
        }
        #endregion

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

                string descriptionFilePath = Path.Combine(this.InstallPath, this.Name, "DESCRIPTION");

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

        internal IReadOnlyList<INamedItemInfo> LoadFunctionInfoFromPackageHelpIndex()
        {
            List<INamedItemInfo> functions = new List<INamedItemInfo>();
            string content = null;

            try
            {
                string htmlFile = Path.Combine(this.InstallPath, this.Name, "html", "00index.html");

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

                FunctionSearch functionSearch = new FunctionSearch(functions);
                tree.Accept(functionSearch, null);
            }

            return functions;
        }

        private class FunctionSearch : IHtmlTreeVisitor
        {
            private List<INamedItemInfo> _functions;

            public FunctionSearch(List<INamedItemInfo> functions)
            {
                _functions = functions;
            }

            public bool Visit(ElementNode element, object parameter)
            {
                if (element.Name.Equals("tr", StringComparison.OrdinalIgnoreCase) && element.Children.Count == 2)
                {
                    ElementNode tdNode1 = element.Children[0];
                    ElementNode tdNode2 = element.Children[1];

                    if (tdNode1.Children.Count == 1 && tdNode1.Children[0].Name.Equals("a", StringComparison.OrdinalIgnoreCase))
                    {
                        string functionName = element.Root.TextProvider.GetText(tdNode1.Children[0].InnerRange);
                        if (IsValidName(functionName))
                        {
                            string functionDescription = element.Root.TextProvider.GetText(tdNode2.InnerRange) ?? string.Empty;
                            _functions.Add(new NamedItemInfo(functionName, functionDescription));
                        }
                    }
                }

                return true;
            }
            private bool IsValidName(string name)
            {
                bool hasCharacters = false;

                if (name == null || name.Length == 0 || !char.IsLetter(name[0]))
                {
                    return false;
                }

                for (int i = 0; i < name.Length; i++)
                {
                    if (char.IsLetterOrDigit(name[i]))
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
