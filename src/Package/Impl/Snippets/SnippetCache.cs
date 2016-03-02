using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Snippets {
    internal sealed class SnippetCache {
        private static string SHORTCUT_LITERAL =
            @"<Literal>
                <ID>shortcut</ID>
                <ToolTip>shortcut</ToolTip>
                <Default>$shortcut$</Default>
            </Literal>";
        private static string SHORTCUT_LITERAL_DECLARATION =
            "<Declarations>" + SHORTCUT_LITERAL + "</Declarations>";

        private static long _lastSequenceId;
        private long _sequenceId;
        private List<VsExpansionEx> _expansions;
        private Dictionary<string, List<SnippetInfo>> _snippetInfoCache;
        private Dictionary<string, SnippetInfoList> _snippetFilePathToInfoMap = new Dictionary<string, SnippetInfoList>();
        private Dictionary<string, SnippetInfoList> _genericSnippetShortcutToInfoMap = new Dictionary<string, SnippetInfoList>(StringComparer.OrdinalIgnoreCase);

        internal long SequenceId => _sequenceId;

        internal bool IsAbandoned { get; set; }

        internal SnippetCache(IVsExpansionManager expansionManager) {
            _sequenceId = SnippetCache._lastSequenceId++;

            // Caching language expansion structs requires access to IVsExpansionManager
            // service that VSP provides. We want to access it on the main thread only,
            // and we don't want the background thread to even have a reference to it.
            // So we will create cache on the singleton object on the main thread, and then
            // simply set the cache field of the temporary object that will run on the 
            // background thread.
            CacheInitialSnippetData(expansionManager);
        }

        private void CacheInitialSnippetData(IVsExpansionManager expansionManager) {
            CacheLanguageExpansionStructs(expansionManager);
        }

        /// <summary>
        /// Caches expansions returned by IVsExpansionManager for a given language services.
        /// </summary>
        /// <remarks>
        /// Needs to happen on the main thread because we are calling into VSP service here,
        /// IVsExpansionManager.
        /// </remarks>
        internal void CacheLanguageExpansionStructs(IVsExpansionManager expansionManager) {
            if(_expansions != null) {
                return;
            }

            IVsExpansionEnumeration expansionEnumeration = null;
            var expansions = new List<VsExpansionEx>();

            int hr = expansionManager.EnumerateExpansions(
                RGuidList.RLanguageServiceGuid,
                0, /* fShortcutsOnly */
                ExpansionClient.GetAllStandardSnippetTypes(),
                ExpansionClient.GetAllStandardSnippetTypes().Length,
                1, /* fIncludeNULLType */
                0, /* fIncludeDuplicates */
                out expansionEnumeration
            );
            ErrorHandler.ThrowOnFailure(hr);

            var buffer = new ExpansionBuffer();
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try {
                uint fetched;
                while (VSConstants.S_OK == (hr = expansionEnumeration.Next(1, new IntPtr[] { handle.AddrOfPinnedObject() }, out fetched))) {
                    var expansion = ConvertToVsExpansionAndFree((ExpansionBuffer)handle.Target);
                    expansions.Add(new VsExpansionEx(expansion));
                }
                ErrorHandler.ThrowOnFailure(hr);
            } finally {
                handle.Free();
            }

            _expansions = expansions;
        }

        private static VsExpansion ConvertToVsExpansionAndFree(ExpansionBuffer buffer) {
            VsExpansion expansion = new VsExpansion();

            ConvertToStringAndFree(ref buffer.descriptionPtr, ref expansion.description);
            ConvertToStringAndFree(ref buffer.pathPtr, ref expansion.path);
            ConvertToStringAndFree(ref buffer.shortcutPtr, ref expansion.shortcut);
            ConvertToStringAndFree(ref buffer.titlePtr, ref expansion.title);

            return expansion;
        }

        private static void ConvertToStringAndFree(ref IntPtr ptr, ref string s) {
            if (IntPtr.Zero != ptr) {
                s = Marshal.PtrToStringBSTR(ptr);
                Marshal.FreeBSTR(ptr);
                ptr = IntPtr.Zero;
            }
        }

        private IEnumerable<XmlNode> ChildrenWithLocalName(XmlNode parent, string name) {
            IEnumerable<XmlNode> childNodes = parent.ChildNodes.Cast<XmlNode>();
            return childNodes.Where(node => node.LocalName.Equals(name));
        }

        private void ParseSnippet(SnippetInfoList snippetInfoListToUpdate) {
            XmlDocument snippetDocument = snippetInfoListToUpdate[0].XmlDocument;
            if (snippetDocument == null && snippetInfoListToUpdate[0].Path != null) {
                snippetDocument = new XmlDocument();
                snippetDocument.Load(snippetInfoListToUpdate[0].Path);
            }

            string[] alternativeShortcuts = null;
            string[] alternativeShortcutValues = null;

            // Let's deal with generic snippets first. Is this a generic one?
            // Generic snippets will have <AlternativeShortcuts> elements in them.

            XmlNodeList alternativeShortcutsNodeList = snippetDocument.GetElementsByTagName("AlternativeShortcuts");
            if (alternativeShortcutsNodeList.Count == 1) {
                XmlNode alternativeShortcutsNode = alternativeShortcutsNodeList[0];
                IEnumerable<XmlNode> shortcutNodes = ChildrenWithLocalName(alternativeShortcutsNode, "Shortcut");
                int count = (shortcutNodes != null ? shortcutNodes.Count() : 0);
                if (count > 0) {
                    int alternativeShortcutIndex = 0;

                    alternativeShortcuts = new string[count];
                    alternativeShortcutValues = new string[count];

                    foreach (XmlNode shortcutNode in shortcutNodes) {
                        string shortcut = shortcutNode.InnerText.Trim();
                        if (shortcut != snippetInfoListToUpdate[0].Shortcut) {
                            alternativeShortcuts[alternativeShortcutIndex] = shortcut;

                            XmlNamedNodeMap shortcutAttributes = shortcutNode.Attributes;
                            XmlNode valueAttribute = shortcutAttributes.GetNamedItem("Value");
                            if (valueAttribute != null) {
                                string value = valueAttribute.InnerText.Trim();
                                alternativeShortcutValues[alternativeShortcutIndex] = value;
                            }

                            alternativeShortcutIndex++;
                        }
                    }
                }
            }

            XmlNodeList projectTypeGuidsNodeList = snippetDocument.GetElementsByTagName("ProjectTypeGuids");

            // There should be only one ProjectTypeGuids node!
            if (projectTypeGuidsNodeList.Count == 1) {
                if (projectTypeGuidsNodeList[0].InnerText != null) {
                    snippetInfoListToUpdate[0].ProjectTypeGuids = new HashSet<Guid>();
                    foreach (string projectTypeGuid in projectTypeGuidsNodeList[0].InnerText.Trim().Split(';')) {
                        snippetInfoListToUpdate[0].ProjectTypeGuids.Add(Guid.Parse(projectTypeGuid));
                    }
                }
            }

            XmlNodeList fileExtensionsNodeList = snippetDocument.GetElementsByTagName("FileExtensions");

            // There should be only one FileExtensions node!
            if (fileExtensionsNodeList.Count == 1) {
                if (fileExtensionsNodeList[0].InnerText != null) {
                    snippetInfoListToUpdate[0].FileExtensions = new HashSet<string>();
                    foreach (string fileExtension in fileExtensionsNodeList[0].InnerText.Trim().Split(';')) {
                        snippetInfoListToUpdate[0].FileExtensions.Add(fileExtension.ToLower(CultureInfo.CurrentCulture));
                    }
                }
            }

            XmlNodeList codeNodeList = snippetDocument.GetElementsByTagName("Code");

            // There should be only one code node!
            if (codeNodeList.Count == 1) {
                XmlNode kind = codeNodeList[0].Attributes.GetNamedItem("Kind");
                if (kind != null) {
                    snippetInfoListToUpdate[0].Kind = kind.InnerText;
                }

                XmlNode key = codeNodeList[0].Attributes.GetNamedItem("Key");
                if (key != null) {
                    snippetInfoListToUpdate[0].Key = key.InnerText;
                }

                XmlNode format = codeNodeList[0].Attributes.GetNamedItem("Format");
                if (format != null) {
                    if (string.Compare(format.InnerText, "False", StringComparison.OrdinalIgnoreCase) == 0) {
                        snippetInfoListToUpdate[0].ShouldFormat = false;
                    }
                }

                // Will hold the value of snippet key inferred by parsing the code.
                string inferredKey = null;

                // This part of the if statement tries to detect both snippet kind
                // and snippet key when either valid kind wasn't specified or
                // the user explicitely told us to guess the kind and the key.
                if (!snippetKinds.IsValidKind(snippetInfoListToUpdate[0].Kind) ||
                    snippetKinds.IsAutoKind(snippetInfoListToUpdate[0].Kind)) {

                    snippetInfoListToUpdate[0].Kind = snippetKinds.DetectKind(
                        codeNodeList[0].InnerText, out inferredKey);

                    // If we still didn't get a valid kind, then mark this snippet as ALL kinds
                    if (!snippetKinds.IsValidKind(snippetInfoListToUpdate[0].Kind)) {
                        snippetInfoListToUpdate[0].Kind = snippetKinds.GetAllSnippetKindName();
                    }
                }
                // This part of the if statement tries to detect snippet key
                // if it wasn't specified by the user and snippet kind is other
                // then "All" ("*"). If snippet kind is "All" ("*"), we cannot
                // possibly guess what the snippet key is as we can guess that
                // only by parsing a specific kind of a snippet.
                else if (!snippetKinds.IsAllKind(snippetInfoListToUpdate[0].Kind) && (snippetInfoListToUpdate[0].Kind == null)) {

                    // Try to get the key string for snippet, but make sure that we actually detect the kind they specified
                    string detectedKind = snippetKinds.DetectKind(codeNodeList[0].InnerText, out inferredKey);
                    // If we think snippet kind is not the same as they specified, then ignore the key string we detected.
                    if (detectedKind != snippetInfoListToUpdate[0].Kind) {
                        snippetInfoListToUpdate[0].Key = null;
                    }
                }

                // If (and only if) the key was not explicitly specpfied,
                // use the key that we inferred by parsing the snippet code.
                if (snippetInfoListToUpdate[0].Key == null)
                    snippetInfoListToUpdate[0].Key = inferredKey;

                // Now expand generic snippet into multiple "simple" snippets
                if (alternativeShortcuts != null) {
                    snippetInfoListToUpdate[0].IsGeneric = true;
                    for (int i = 0; i < alternativeShortcuts.Length; i++) {
                        if (snippetInfoListToUpdate.Count < i + 2) {
                            SnippetInfo snippetInfo = new SnippetInfo(snippetInfoListToUpdate[0]);
                            snippetInfoListToUpdate.Add(snippetInfo);
                            Debug.Assert(snippetInfoListToUpdate.Count == i + 2, "Unexpected number of snippets in snippet list.");
                        } else {
                            snippetInfoListToUpdate[i + 1].CopyFrom(snippetInfoListToUpdate[0]);
                        }
                        snippetInfoListToUpdate[i + 1].Shortcut = alternativeShortcuts[i];
                        string value = alternativeShortcuts[i];
                        if (alternativeShortcutValues[i] != null) {
                            value = alternativeShortcutValues[i];
                            snippetInfoListToUpdate[i + 1].Value = value;
                        }

                        if (snippetInfoListToUpdate[i + 1].Key != null) {
                            snippetInfoListToUpdate[i + 1].Key = snippetInfoListToUpdate[i + 1].Key.Replace("$shortcut$", value);
                        }
                        AddSnippetToShortcutMapping(snippetInfoListToUpdate[i + 1]);
                    }

                    if (snippetInfoListToUpdate[0].Key != null) {
                        snippetInfoListToUpdate[0].Key = snippetInfoListToUpdate[0].Key.Replace("$shortcut$", snippetInfoListToUpdate[0].Shortcut);
                    }
                    AddSnippetToShortcutMapping(snippetInfoListToUpdate[0]);
                }
            }
        }

        private void AddSnippetToShortcutMapping(SnippetInfo snippetInfo) {
            SnippetInfoList snippetInfoListForShortcut = null;
            if (!_genericSnippetShortcutToInfoMap.TryGetValue(snippetInfo.Shortcut, out snippetInfoListForShortcut)) {
                snippetInfoListForShortcut = new SnippetInfoList();
                _genericSnippetShortcutToInfoMap.Add(snippetInfo.Shortcut, snippetInfoListForShortcut);
            }
            snippetInfoListForShortcut.Add(snippetInfo);
        }

        #region ISnippetListManager implementation

        public SnippetInfo[] GetSnippetList(string projectTypeGuids, string fileExtension, Guid languageGuid, string kind) {
            // skip the '.'
            if (fileExtension.Length > 0 && fileExtension[0] == '.') {
                fileExtension = fileExtension.Substring(1);
            }

            List<SnippetInfo> result = new List<SnippetInfo>();
            Dictionary<string, List<SnippetInfo>> snippetListPerKind = _snippetInfoCache;

            if (snippetListPerKind != null) {

                List<SnippetInfo> snippetList = null;

                if (snippetListPerKind.TryGetValue(kind, out snippetList)) {
                    List<SnippetInfo> filteredSnippetList = new List<SnippetInfo>();
                    HashSet<Guid> currentProjectTypeGuids = new HashSet<Guid>();

                    if (projectTypeGuids != null) {
                        foreach (string projectTypeGuid in projectTypeGuids.Trim().Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)) {
                            currentProjectTypeGuids.Add(Guid.Parse(projectTypeGuid));
                        }
                    }

                    foreach (SnippetInfo snippetInfo in snippetList) {
                        bool addSnippetInfo = true;
                        if (snippetInfo.ProjectTypeGuids != null) {
                            foreach (Guid snippetProjectTypeGuid in snippetInfo.ProjectTypeGuids) {
                                if (!currentProjectTypeGuids.Contains(snippetProjectTypeGuid)) {
                                    addSnippetInfo = false;
                                    break;
                                }
                            }
                        }

                        if (snippetInfo.FileExtensions != null && snippetInfo.FileExtensions.Count > 0) {
                            if (!snippetInfo.FileExtensions.Contains(fileExtension.ToLower(CultureInfo.CurrentCulture))) {
                                addSnippetInfo = false;
                            }
                        }

                        if (addSnippetInfo) {
                            filteredSnippetList.Add(snippetInfo);
                        }
                    }
                    result.AddRange(filteredSnippetList);
                }
            }

            return result.ToArray();
        }

        public void GetGenericSnippetInfo(string shortcut, string path, out MSXML.IXMLDOMNode snippetXMLNode, out string relativePath) {
            snippetXMLNode = null;
            relativePath = null;

            SnippetInfoList snippetInfoList;
            Dictionary<string, SnippetInfoList> genericSnippetShortcutToInfoMap = _genericSnippetShortcutToInfoMap;

            if (genericSnippetShortcutToInfoMap.TryGetValue(shortcut, out snippetInfoList)) {
                // The specifc template instance that the user actually wants.
                // If there is a path, then there might be more than one snippet for the specified
                // shortcut, and we want to pick that one. Otherwise just take the first one from the
                // snippet list.
                SnippetInfo specificSnippetInfo = null;
                if (path != null) {
                    foreach (SnippetInfo snippetInfo in snippetInfoList) {
                        if (path.Equals(snippetInfo.Path, StringComparison.OrdinalIgnoreCase)) {
                            specificSnippetInfo = snippetInfo;
                            break;
                        }
                    }
                } else {
                    specificSnippetInfo = snippetInfoList[0];
                }

                relativePath = specificSnippetInfo.Path;
                StringBuilder snippetXMLBuilder = null;
                if (specificSnippetInfo.Path != null) {
                    snippetXMLBuilder = new StringBuilder(System.IO.File.ReadAllText(relativePath));
                } else if (specificSnippetInfo.XmlDocument != null) {
                    snippetXMLBuilder = new StringBuilder(specificSnippetInfo.XmlDocument.OuterXml);
                } else {
                    return;
                }

                string value = shortcut;

                if (specificSnippetInfo.Value != null) {
                    value = specificSnippetInfo.Value;
                }

                string snippetXML = snippetXMLBuilder.Replace("$shortcut$", value).ToString();

                // This should be done on the main thread! DomDocumentClass isn't thread-safe.
                snippetXMLNode = XmlUtilities.CreateAndLoadMsXmlDom60SnippetDocument(snippetXML);
            }
        }

        public void GetGenericSnippetFileInfo(string filePath, out MSXML.IXMLDOMNode snippetXMLNode, out string relativePath) {
            if (filePath == null) {
                snippetXMLNode = null;
                relativePath = null;
                return;
            }

            relativePath = filePath;
            XmlDocument snippetDocument = new XmlDocument();
            snippetDocument.Load(filePath);

            Dictionary<string, SnippetInfoList> snippetFilePathToInfoMapSnapshot = _snippetFilePathToInfoMap;

            string value = "shortcut";
            SnippetInfoList infoList = null;
            if (snippetFilePathToInfoMapSnapshot.TryGetValue(filePath, out infoList)) {
                if (infoList.Count > 0) {
                    value = infoList[0].Shortcut;
                    if (infoList[0].Value != null) {
                        value = infoList[0].Value;
                    }
                }
            }

            XmlNodeList declarations = snippetDocument.GetElementsByTagName("Declarations");
            if ((declarations == null) || (declarations.Count == 0)) {
                XmlNodeList snippet = snippetDocument.GetElementsByTagName("Snippet");
                if ((snippet != null) || (snippet.Count > 0)) {
                    XmlDocumentFragment fragment = snippetDocument.CreateDocumentFragment();
                    fragment.InnerXml = SHORTCUT_LITERAL_DECLARATION.Replace("$shortcut$", value);
                    snippet[0].InsertAfter(fragment, null);
                }
            } else {
                XmlDocumentFragment fragment = snippetDocument.CreateDocumentFragment();
                fragment.InnerXml = SHORTCUT_LITERAL.Replace("$shortcut$", value);
                declarations[0].InsertAfter(fragment, null);
            }
            // Calling insertBefore adds xmlns="" to the node, which messes up VSP. Remove it, and reload.
            string snippetXML = snippetDocument.OuterXml.Replace("xmlns=\"\"", String.Empty);

            MSXML.IXMLDOMDocument msxmlSnippetDocument = XmlUtilities.CreateMsXmlDom60Document();
            msxmlSnippetDocument.loadXML(snippetXML);

            snippetXMLNode = msxmlSnippetDocument;
        }

        public bool IsGenericSnippet(string shortcut) {
            var genericSnippetShortcutToInfoMap = _genericSnippetShortcutToInfoMap;
            return genericSnippetShortcutToInfoMap.ContainsKey(shortcut);
        }

        public bool IsGenericSnippetFile(string filePath) {
            bool result = false;
            SnippetInfoList snippetInfoList = null;

            Dictionary<string, SnippetInfoList> snippetFilePathToInfoMapSnapshot = _snippetFilePathToInfoMap;

            if (snippetFilePathToInfoMapSnapshot.TryGetValue(filePath, out snippetInfoList)) {
                if ((snippetInfoList.Count > 0) && snippetInfoList[0].IsGeneric) {
                    result = true;
                }
            }

            return result;
        }
        #endregion
    }
}

