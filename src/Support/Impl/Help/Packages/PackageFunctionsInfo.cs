using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Html.Core.Tree;
using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Utility;

namespace Microsoft.R.Support.Help.Packages
{
    public sealed class PackageFunctionsInfo : AsyncDataSource<IReadOnlyList<string>>
    {
        private string _packageName;

        public IReadOnlyCollection<INamedItemInfo> Functions
        {
            get
            {
                FunctionIndex.GetPackageFunctions(this)
                if (_functions == null)
                {
                    _functions = GetFunctions();
                }

                return _functions.Values;
            }
        }

        public PackageFunctionsInfo(string packageName, bool isBase)
        {
            _packageName = packageName;
            InstallPath = installPath;
            this.DataReady += OnDataReady;
        }

        private void OnDataReady(object sender, string e)
        {
            throw new NotImplementedException();
        }

        private Dictionary<string, INamedItemInfo> GetFunctions()
        {
            Dictionary<string, INamedItemInfo> functions = new Dictionary<string, INamedItemInfo>();
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

                FunctionSearch functionSearch = new FunctionSearch(functions);
                tree.Accept(functionSearch, null);
            }

            return functions;
        }

        private class FunctionSearch : IHtmlTreeVisitor
        {
            private Dictionary<string, INamedItemInfo> _functions;

            public FunctionSearch(Dictionary<string, INamedItemInfo> functions)
            {
                _functions = functions;
            }

            public bool Visit(ElementNode element, object parameter)
            {
                if (element.Name.Equals("tr", StringComparison.OrdinalIgnoreCase) && element.Children.Count == 2)
                {
                    var functionInfo = new NamedItemInfo();

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
                                _functions[functionInfo.Name] = functionInfo;
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
