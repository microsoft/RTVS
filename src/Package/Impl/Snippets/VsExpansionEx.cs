using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Snippets {
    internal class VsExpansionEx {
        private VsExpansion VsExpansion;
        public XmlDocument document { get; private set; }

        internal VsExpansionEx(VsExpansion expansion) {
            VsExpansion = expansion;
        }

        internal VsExpansionEx(XmlDocument snippetDocument) {
            document = snippetDocument;
        }

        public string Description => VsExpansion.description;
        public string Path => VsExpansion.path;
        public string Shortcut => VsExpansion.shortcut;
        public string Title => VsExpansion.title;

        private XmlNode FirstChildWithLocalName(XmlNode parent, string name) {
            IEnumerable<XmlNode> childNodes = parent.ChildNodes.Cast<XmlNode>();
            return childNodes.FirstOrDefault(node => node.LocalName.Equals(name));
        }

        public void EnsurePopulated() {
            if (document != null) {
                XmlNode rootNode = document.DocumentElement;
                XmlNode headerNode = FirstChildWithLocalName(rootNode, "Header");

                XmlNode shortcutNode = FirstChildWithLocalName(headerNode, "Shortcut");
                if (shortcutNode != null) {
                    VsExpansion.shortcut = shortcutNode.InnerText;
                }

                XmlNode descriptionNode = FirstChildWithLocalName(headerNode, "Description");
                if (descriptionNode != null) {
                    VsExpansion.description = descriptionNode.InnerText;
                }

                XmlNode titleNode = FirstChildWithLocalName(headerNode, "Title");
                if (titleNode != null) {
                    VsExpansion.title = titleNode.InnerText;
                }
            }
        }
    }
}
