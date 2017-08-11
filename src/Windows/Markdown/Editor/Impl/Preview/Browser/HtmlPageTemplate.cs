// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Reflection;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;

namespace Microsoft.Markdown.Editor.Preview.Browser {
    internal static class HtmlPageTemplate {
        private static string _htmlTemplate;

        public static string GetPageHtml(IFileSystem fs, string body) {
            if (_htmlTemplate == null) {
                var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetAssemblyPath());
                var path = Path.Combine(dir, "Markdown", "PreviewTemplate.html");
                _htmlTemplate = fs.ReadAllText(path);
            }
            return _htmlTemplate.Replace("_BODY_", body);
        }
    }
}
