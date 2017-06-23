// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Markdig.Extensions.Yaml;
using Markdig.Renderers;
using Microsoft.Common.Core;
using Microsoft.R.Core.AST.Statements.Loops;
using YamlDotNet.RepresentationModel;
using static System.FormattableString;

namespace Microsoft.Markdown.Editor.Preview.Parser {
    internal sealed class YamlRenderer : YamlFrontMatterRenderer {
        protected override void Write(HtmlRenderer renderer, YamlFrontMatterBlock block) {
            try {
                var yaml = new YamlStream();
                yaml.Load(new StringReader(block.GetText()));
                var root = (YamlMappingNode)yaml.Documents[0].RootNode;
                WriteCss(renderer, root);
                WriteTitle(renderer, root);
            } catch (Exception ex) when (!ex.IsCriticalException()) { }

            base.Write(renderer, block);
        }

        private void WriteTitle(HtmlRenderer renderer, YamlMappingNode root) {
            var titleNode = GetNode(root, new[] { "title" }) as YamlScalarNode;
            if (!string.IsNullOrEmpty(titleNode?.Value)) {
                renderer.Write(Invariant($"<h1>{titleNode.Value}</h1>"));
            }
        }
        private void WriteCss(HtmlRenderer renderer, YamlMappingNode root) {
            var htmlDocNode = GetNode(root, new[] { "output", "html_document" }) as YamlMappingNode;
            if (htmlDocNode != null) {
                htmlDocNode.Children.TryGetValue(new YamlScalarNode("theme"), out YamlNode node);
                var themeNode = node as YamlScalarNode;
                //htmlDocNode.Children.TryGetValue(new YamlScalarNode("highlight"), out YamlNode highlightNode);

                if(!string.IsNullOrEmpty(themeNode?.Value) && !themeNode.Value.EqualsIgnoreCase("null")) {
                    var url = Invariant($"http://bootswatch.com/{themeNode.Value}/bootstrap.min.css");
                    renderer.Write(FormatStyleSheetLink(url));
                } else {
                    htmlDocNode.Children.TryGetValue(new YamlScalarNode("css"), out node);
                    var cssNode = node as YamlScalarNode;
                    if (!string.IsNullOrEmpty(cssNode?.Value)) {
                        renderer.Write(FormatStyleSheetLink(cssNode.Value));
                    }
                }
            }
        }

        private static YamlNode GetNode(YamlMappingNode root, IEnumerable<string> selectors) {
            YamlNode childNode = null;
            var node = root;
            foreach (var selector in selectors) {
                if (node == null || !node.Children.TryGetValue(new YamlScalarNode(selector), out childNode)) {
                    return null;
                }
                node = childNode as YamlMappingNode;
            }
            return childNode;
        }

        private string FormatStyleSheetLink(string url)
            => Invariant($"<link rel='stylesheet' type='text/css' href='{url}' />");
    }
}
