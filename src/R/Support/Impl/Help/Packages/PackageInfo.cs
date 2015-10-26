using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Html.Core.Tree;
using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;

namespace Microsoft.R.Support.Help.Packages {
    /// <summary>
    /// Represents R package installed on user machine
    /// </summary>
    public sealed class PackageInfo : NamedItemInfo, IPackageInfo {
        private string _description;

        public PackageInfo(string name, string installPath) :
            base(name, NamedItemType.Package) {
            InstallPath = installPath;
        }

        /// <summary>
        /// Package description
        /// </summary>
        #region NamedItemInfo
        public override string Description {
            get {
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
        public IReadOnlyCollection<INamedItemInfo> Functions {
            get {
                IReadOnlyCollection<INamedItemInfo> functions = FunctionIndex.GetPackageFunctions(this.Name);
                if (functions == null || functions.Count == 0) {
                    functions = LoadFunctionInfoFromPackageHelpIndex();
                }

                return functions;
            }
        }
        #endregion

        /// <summary>
        /// Extract package description from the DESCRITION file on disk
        /// </summary>
        private string GetDescription() {
            StringBuilder sb = new StringBuilder();
            bool found = false;

            try {
                // DESCRIPTION uses a simple file format called DCF, the Debian control format. 
                // Each line consists of a field name and a value, separated by a colon.
                // When values span multiple lines, they need to be indented:
                //
                // Description: The description of a package is usually long,
                //    spanning multiple lines. The second and subsequent lines
                //    should be indented, usually with four spaces.

                string descriptionFilePath = Path.Combine(this.InstallPath, this.Name, "DESCRIPTION");

                using (StreamReader sr = new StreamReader(descriptionFilePath, Encoding.UTF8)) {
                    while (!found) {
                        string line = sr.ReadLine();
                        if (line == null) {
                            break;
                        }

                        if (line.StartsWith("Description:", StringComparison.OrdinalIgnoreCase)) {
                            line = line.Substring(12).Trim();
                            sb.Append(line.Trim());
                            sb.Append(' ');

                            while (!found) {
                                line = sr.ReadLine();
                                if (line == null) {
                                    break;
                                }

                                if (line.Length > 0 && char.IsWhiteSpace(line[0])) {
                                    line.Trim();

                                    sb.Append(line.Trim());
                                    sb.Append(' ');
                                } else {
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            } catch (IOException) { }

            return sb.ToString().Trim();
        }

        internal IReadOnlyList<INamedItemInfo> LoadFunctionInfoFromPackageHelpIndex() {
            List<INamedItemInfo> functions = new List<INamedItemInfo>();
            string content = null;

            try {
                string htmlFile = Path.Combine(this.InstallPath, this.Name, "html", "00index.html");
                if (File.Exists(htmlFile)) {
                    using (StreamReader sr = new StreamReader(htmlFile, Encoding.UTF8)) {
                        content = sr.ReadToEnd();
                    }
                }
            } catch (IOException) { }

            if (!string.IsNullOrEmpty(content)) {
                HtmlTree tree = new HtmlTree(new Microsoft.Web.Core.Text.TextStream(content));
                tree.Build();

                FunctionSearch functionSearch = new FunctionSearch(functions);
                tree.Accept(functionSearch, null);
            }

            Dictionary<string, INamedItemInfo> functionIndex = new Dictionary<string, INamedItemInfo>();
            foreach (INamedItemInfo ni in functions) {
                functionIndex[ni.Name] = ni;
            }

            IReadOnlyDictionary<string, string> mappedNames = GetMappedNames();
            foreach (string mappedName in mappedNames.Keys) {
                INamedItemInfo ni;
                string actualName = mappedNames[mappedName];
                if (functionIndex.TryGetValue(actualName, out ni)) {
                    INamedItemInfo niAlias = new NamedItemInfo() {
                        Name = mappedName,
                        Description = ni.Description,
                        ItemType = ni.ItemType
                    };
                    functions.Add(niAlias);
                }
            }

            return functions;
        }

        private IReadOnlyDictionary<string, string> GetMappedNames() {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            try {
                string indexFile = Path.Combine(this.InstallPath, this.Name, "help", "AnIndex");
                if (File.Exists(indexFile)) {
                    using (StreamReader sr = new StreamReader(indexFile, Encoding.UTF8)) {
                        string line;
                        char[] splitChars = new char[] { ' ', '\t' };
                        while ((line = sr.ReadLine()) != null) {
                            string[] parts = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length == 2) {
                                string mappedName = parts[0];
                                string actualName = parts[1];
                                if (mappedName != actualName) {
                                    dict[mappedName] = actualName;
                                }
                            }
                        }
                    }
                }
            } catch (IOException) { }

            return dict;
        }

        private class FunctionSearch : IHtmlTreeVisitor {
            private List<INamedItemInfo> _functions;

            public FunctionSearch(List<INamedItemInfo> functions) {
                _functions = functions;
            }

            public bool Visit(ElementNode element, object parameter) {
                if (element.Name.Equals("tr", StringComparison.OrdinalIgnoreCase) && element.Children.Count == 2) {
                    ElementNode tdNode1 = element.Children[0];
                    ElementNode tdNode2 = element.Children[1];

                    if (tdNode1.Children.Count == 1 && tdNode1.Children[0].Name.Equals("a", StringComparison.OrdinalIgnoreCase)) {
                        string functionName = element.Root.TextProvider.GetText(tdNode1.Children[0].InnerRange);
                        if (functionName.IndexOf('&') >= 0) {
                            functionName = WebUtility.HtmlDecode(functionName);
                        } else if (!IsValidName(functionName)) {
                            return true;
                        }

                        NamedItemType itemType = GetItemType(functionName, tdNode1);
                        if (itemType != NamedItemType.None) {
                            string functionDescription = element.Root.TextProvider.GetText(tdNode2.InnerRange) ?? string.Empty;
                            _functions.Add(new NamedItemInfo(functionName, functionDescription, itemType));
                        }
                    }
                }

                return true;
            }

            private static NamedItemType GetItemType(string name, ElementNode td) {
                if (Constants.IsConstant(name) || Logicals.IsLogical(name) || name.StartsWith("R_", StringComparison.OrdinalIgnoreCase)) {
                    return NamedItemType.Constant;
                }

                if (td.Children.Count == 1) {
                    ElementNode a = td.Children[0];
                    AttributeNode href = a.GetAttribute("href");

                    if (href != null && href.Value != null) {
                        if (href.Value.IndexOf("constant", StringComparison.OrdinalIgnoreCase) >= 0) {
                            return NamedItemType.Constant;
                        } else if (href.Value.IndexOf("-package", StringComparison.OrdinalIgnoreCase) >= 0) {
                            return NamedItemType.None;
                        }
                    }
                }

                return NamedItemType.Function;
            }

            private bool IsValidName(string name) {
                bool hasCharacters = false;

                if (name == null || name.Length == 0) {
                    return false;
                }

                for (int i = 0; i < name.Length; i++) {
                    if (char.IsLetterOrDigit(name[i])) {
                        hasCharacters = true;
                        break;
                    }
                }

                return hasCharacters;
            }
        }
    }
}
