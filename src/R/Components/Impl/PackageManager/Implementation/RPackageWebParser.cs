// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Html.Core.Tree;
using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.R.Components.PackageManager.Model;
using Microsoft.R.Components.Test.PackageManager;

namespace Microsoft.R.Components.PackageManager.Implementation {
    internal class RPackageWebParser {
        /// <summary>
        /// Downloads the index html page specified, and extracts a set of known fields from it.
        /// </summary>
        /// <param name="packageUri">Location of index.html page.</param>
        /// <param name="result">RPackage object to fill in. A new one is created if none is passed in.</param>
        /// <returns>RPackage object populated with the information found in the html page.</returns>
        /// <remarks>
        /// If an existing RPackage object is passed in, the fields that are already filled in
        /// won't be overwritten by the ones extracted from the html page.
        /// </remarks>
        public static async Task<RPackage> RetrievePackageInfo(Uri packageUri, RPackage result = null) {
            string content;
            try {
                using (var webClient = new WebClient()) {
                    content = await webClient.DownloadStringTaskAsync(packageUri);
                }
            } catch (WebException ex) {
                throw new RPackageInfoRetrievalException(string.Format("Error reading index page from {0}", packageUri), ex);
            }

            var pkg = result ?? new RPackage();
            try {
                var tree = new HtmlTree(new Microsoft.Web.Core.Text.TextStream(content));
                tree.Build();

                var search = new IndexVisitor(pkg);
                tree.Accept(search, null);
            } catch (Exception ex) when(!ex.IsCriticalException()) {
                throw new RPackageInfoRetrievalException(string.Format("Error parsing index page from {0}", packageUri), ex);
            }

            return pkg;
        }

        /// <summary>
        /// The names of the fields as they appear in the html page.
        /// </summary>
        private static class Fields {
            public const string Depends = "Depends";
            public const string Imports = "Imports";
            public const string Suggests = "Suggests";
            public const string License = "License";
            public const string Version = "Version";
            public const string NeedsCompilation = "NeedsCompilation";
            public const string Author = "Author";
            public const string URL = "URL";
            public const string LinkingTo = "LinkingTo";
            public const string Enhances = "Enhances";
            public const string Maintainer = "Maintainer";
            public const string BugReports = "BugReports";
            public const string Published = "Published";
        }

        private class IndexVisitor : IHtmlTreeVisitor {
            private RPackage _pkg;

            public IndexVisitor(RPackage pkg) {
                _pkg = pkg;
            }

            public bool Visit(ElementNode element, object parameter) {
                if (element.Name.EqualsIgnoreCase("h2") &&
                    element.Parent.Name.EqualsIgnoreCase("body")) {
                    ParseNameAndTitleFields(element);
                    return true;
                }

                if (element.Name.EqualsIgnoreCase("p") &&
                    element.Parent.Name.EqualsIgnoreCase("body")) {
                    ParseDescriptionField(element);
                    return true;
                }

                if (element.Name.EqualsIgnoreCase("tr") && element.Children.Count == 2 &&
                    element.Children[0].Name.EqualsIgnoreCase("td") &&
                    element.Children[1].Name.EqualsIgnoreCase("td")) {
                    ParseGenericField(element.Children[0], element.Children[1]);
                    return true;
                }

                return true;
            }

            private void ParseNameAndTitleFields(ElementNode h2Node) {
                string text = GetElementText(h2Node);
                int separatorIndex = text.IndexOf(':');
                if (separatorIndex >= 0) {
                    if (string.IsNullOrEmpty(_pkg.Package)) {
                        _pkg.Package = text.Substring(0, separatorIndex).Trim();
                    }

                    if (string.IsNullOrEmpty(_pkg.Title)) {
                        _pkg.Title = text.Substring(separatorIndex + 1).Trim();
                    }
                }
            }

            private void ParseDescriptionField(ElementNode paragraphNode) {
                if (string.IsNullOrEmpty(_pkg.Description)) {
                    _pkg.Description = GetElementText(paragraphNode);
                }
            }

            private void ParseGenericField(ElementNode tdNode1, ElementNode tdNode2) {
                string fieldName = GetElementText(tdNode1).Trim(':');
                string fieldValue = GetElementText(tdNode2);

                switch (fieldName) {
                    case Fields.Author:
                        if (string.IsNullOrEmpty(_pkg.Author)) {
                            _pkg.Author = fieldValue;
                        }
                        break;
                    case Fields.BugReports:
                        if (string.IsNullOrEmpty(_pkg.BugReports)) {
                            _pkg.BugReports = fieldValue;
                        }
                        break;
                    case Fields.Depends:
                        if (string.IsNullOrEmpty(_pkg.Depends)) {
                            _pkg.Depends = fieldValue;
                        }
                        break;
                    case Fields.Enhances:
                        if (string.IsNullOrEmpty(_pkg.Enhances)) {
                            _pkg.Enhances = fieldValue;
                        }
                        break;
                    case Fields.Imports:
                        if (string.IsNullOrEmpty(_pkg.Imports)) {
                            _pkg.Imports = fieldValue;
                        }
                        break;
                    case Fields.License:
                        if (string.IsNullOrEmpty(_pkg.License)) {
                            _pkg.License = fieldValue;
                        }
                        break;
                    case Fields.LinkingTo:
                        if (string.IsNullOrEmpty(_pkg.LinkingTo)) {
                            _pkg.LinkingTo = fieldValue;
                        }
                        break;
                    case Fields.Maintainer:
                        if (string.IsNullOrEmpty(_pkg.Maintainer)) {
                            _pkg.Maintainer = fieldValue;
                        }
                        break;
                    case Fields.NeedsCompilation:
                        if (string.IsNullOrEmpty(_pkg.NeedsCompilation)) {
                            _pkg.NeedsCompilation = fieldValue;
                        }
                        break;
                    case Fields.Published:
                        if (string.IsNullOrEmpty(_pkg.Published)) {
                            _pkg.Published = fieldValue;
                        }
                        break;
                    case Fields.Suggests:
                        if (string.IsNullOrEmpty(_pkg.Suggests)) {
                            _pkg.Suggests = fieldValue;
                        }
                        break;
                    case Fields.URL:
                        if (string.IsNullOrEmpty(_pkg.URL)) {
                            _pkg.URL = fieldValue;
                        }
                        break;
                    case Fields.Version:
                        if (string.IsNullOrEmpty(_pkg.Version)) {
                            _pkg.Version = fieldValue;
                        }
                        break;
                    default:
                        // There are plenty of other fields available in the html
                        // we just don't care about them all.
                        break;
                }
            }

            private static string GetElementText(ElementNode element) {
                string text = element.Root.TextProvider.GetText(element.InnerRange).Trim();

                // Replace the child nodes with just their text. Example:
                // '<a href="link">text</a>' is replaced with 'text'
                foreach (var child in element.Children) {
                    var outer = element.Root.TextProvider.GetText(child.OuterRange);
                    var inner = element.Root.TextProvider.GetText(child.InnerRange);
                    text = text.Replace(outer, inner);
                }

                if (text.IndexOf('&') >= 0) {
                    text = WebUtility.HtmlDecode(text);
                }

                text = text.Replace("\r\n", "\n");
                text = text.Replace("\n", " ").Trim();

                return text;
            }
        }
    }
}
