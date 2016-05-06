// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Html.Core.Parser;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Tree.Builder {
    public sealed class HtmlTreeBuilder {
        private class ElementRecord {
            public ElementNode Element { get; private set; }
            public List<ElementNode> Children { get; private set; }
            public List<AttributeNode> StartTagAttributes { get; private set; }
            public List<AttributeNode> EndTagAttributes { get; private set; }

            public ElementRecord(ElementNode element) {
                Element = element;
                Children = new List<ElementNode>();
                StartTagAttributes = new List<AttributeNode>();
                EndTagAttributes = new List<AttributeNode>();
            }
        }

        public HtmlParser Parser { get; private set; }
        public TimeSpan TreeBuildingTime { get; private set; }

        private HtmlTree _tree;
        private ElementRecord _rootNodeRecord;
        private ElementRecord _currentElementRecord;
        private Stack<ElementRecord> _elementStack;
        private ElementRecord _orphanedEndTagRecord;

        private DateTime _startTime;

        #region Constructors
        public HtmlTreeBuilder(HtmlParser parser, HtmlTree tree) {
            Parser = parser;
            _tree = tree;

            parser.ParsingStarting += OnParseBegin;
            parser.ParsingComplete += OnParseEnd;
            parser.StartTagOpen += OnStartTagOpen;
            parser.StartTagClose += OnStartTagClose;
            parser.AttributeFound += OnAttribute;
            parser.EndTagOpen += OnEndTagOpen;
            parser.EndTagClose += OnEndTagClose;

            parser.CommentFound += OnComment;
            // For tree building purposes we don't care about comments or entities.
            // Colorable item collection cares so it should be listening to those events.
        }
        #endregion

        #region Begin and end session
        private void OnParseBegin(object sender, HtmlParserRangeEventArgs e) {
            _startTime = DateTime.UtcNow;
            _rootNodeRecord = new ElementRecord(new RootNode(_tree));
            _currentElementRecord = null;
            _orphanedEndTagRecord = null;
            _elementStack = new Stack<ElementRecord>(16);
            _elementStack.Push(_rootNodeRecord);
        }

        private void OnParseEnd(object sender, HtmlParserRangeEventArgs e) {
            // Collect all orphaned end tags since they belong to the currently
            // 
            CloseOrphanedEndTag(new TextRange(e.Range.End, 0), false);

            // Close all element that are still on the stack
            while (_elementStack.Count > 0) {
                ElementRecord elementRecord = _elementStack.Pop();

                ElementNode element = elementRecord.Element;
                ReadOnlyCollection<ElementNode> children = ElementNode.EmptyCollection;
                if (elementRecord.Children.Count > 0) {
                    children = new ReadOnlyCollection<ElementNode>(elementRecord.Children);
                }

                ReadOnlyCollection<AttributeNode> startTagAttributes = AttributeNode.EmptyCollection;
                if (elementRecord.StartTagAttributes.Count > 0) {
                    startTagAttributes = new ReadOnlyCollection<AttributeNode>(elementRecord.StartTagAttributes);
                }

                ReadOnlyCollection<AttributeNode> endTagAttributes = AttributeNode.EmptyCollection;
                if (elementRecord.EndTagAttributes.Count > 0) {
                    endTagAttributes = new ReadOnlyCollection<AttributeNode>(elementRecord.EndTagAttributes);
                }

                element.CompleteElement(TextRange.FromBounds(e.Range.End, e.Range.End), false, children, startTagAttributes, endTagAttributes);
            }

            _tree.RootNode = _rootNodeRecord.Element as RootNode;
            _rootNodeRecord = null;

            _tree.CommentCollection.Sort();

            var parser = sender as HtmlParser;
            _tree.DocType = parser.DocType;

            TreeBuildingTime = DateTime.UtcNow - _startTime;
        }
        #endregion

        private void OnStartTagOpen(object sender, HtmlParserOpenTagEventArgs e) {
            Debug.Assert(_currentElementRecord == null);

            // e.NameToken describes element prefix:name range. 
            // < precedes the range immediately. 
            // Artifacts are not allowed in element names.

            string[] containerNames;

            // Close any elements on the stack that must be closed either by implicit
            // closure or by the fact that they are inside the element that is being closed.
            if (e.NameToken != null && e.NameToken.IsNameWellFormed() &&
                 _tree.HtmlClosureProvider.IsImplicitlyClosed(_tree.Text, e.NameToken, out containerNames)) {
                // close at name.start-1 (skip < of the tag, end exclusive)
                CloseElements(e.NameToken.QualifiedName, e.NameToken.QualifiedName.Start - 1, containerNames);
            }

            // Parent is the element on top of the stack or the root node if stack is empty
            var parentRecord = _elementStack.Count > 0 ? _elementStack.Peek() : _rootNodeRecord;

            ElementNode element = new ElementNode(parentRecord.Element, e.OpenAngleBracketPosition, e.NameToken, _tree.Text.Length);

            parentRecord.Children.Add(element);
            _currentElementRecord = new ElementRecord(element);
        }

        private void OnStartTagClose(object sender, HtmlParserCloseTagEventArgs e) {
            // e.Token contain Token with range for > or /> if well formed
            // or IsGhost is set to true otherwise

            Debug.Assert(_currentElementRecord != null);

            // Close start tag of the current element and push element
            // on the stack if element is not self-closing (or self-closed via />).

            ElementNode currentElement = _currentElementRecord.Element;
            ReadOnlyCollection<AttributeNode> attributes = AttributeNode.EmptyCollection;
            if (_currentElementRecord.StartTagAttributes.Count > 0) {
                attributes = new ReadOnlyCollection<AttributeNode>(_currentElementRecord.StartTagAttributes);
            }

            if (currentElement.StartTag.NameToken == null ||
                currentElement.Name.Equals("!doctype", StringComparison.OrdinalIgnoreCase)) {
                currentElement.StartTag.Complete(attributes, e.CloseAngleBracket, e.IsClosed, e.IsShorthand, true);

                currentElement.CompleteElement(e.CloseAngleBracket, e.IsClosed,
                    ElementNode.EmptyCollection,
                    attributes,
                    AttributeNode.EmptyCollection);
            } else {
                // Check if element is self-closing by calling URI info provider for a given namespace.
                bool selfClosing = false;

                NameToken nameToken = currentElement.StartTag.NameToken;
                if (nameToken != null) {
                    selfClosing = _tree.HtmlClosureProvider.IsSelfClosing(_tree.Text, nameToken.PrefixRange, nameToken.NameRange);
                }

                currentElement.StartTag.Complete(attributes, e.CloseAngleBracket, e.IsClosed, e.IsShorthand, selfClosing);

                currentElement.CompleteElement(e.CloseAngleBracket, e.IsClosed,
                        ElementNode.EmptyCollection,
                        attributes,
                        AttributeNode.EmptyCollection);

                if (!selfClosing && !e.IsShorthand)
                    _elementStack.Push(_currentElementRecord);
            }

            _currentElementRecord = null;
        }

        private void OnAttribute(object sender, HtmlParserAttributeEventArgs e) {
            if (_currentElementRecord != null) {
                var currentElement = _currentElementRecord.Element;
                var a = AttributeNode.Create(currentElement, e.AttributeToken);

                List<AttributeNode> attributeCollection;

                if (currentElement.StartTag.IsComplete)
                    attributeCollection = _currentElementRecord.EndTagAttributes;
                else
                    attributeCollection = _currentElementRecord.StartTagAttributes;

                attributeCollection.Add(a);
            }
        }

        private void OnEndTagOpen(object sender, HtmlParserOpenTagEventArgs e) {
            // Current element must already be on the stack. It is not null
            // only when we are processing start tag.

            Debug.Assert(_currentElementRecord == null);

            // e.Token contains NameToken describing element prefix:name range. 
            // </ precedes the range immediately. Note that token may contain invalid/empty 
            // range if element name is missing such as in </{ws} or </>. We will ignore
            // malformed end tags.

            if (e.NameToken != null && e.NameToken.QualifiedName.Length > 0) {
                // close at name.start-1 (skip < of the tag, end exclusive)

                // subtract </
                _currentElementRecord = CloseElements(e.NameToken.QualifiedName, e.NameToken.QualifiedName.Start - 2, new string[0]);
                if (_currentElementRecord != null) {
                    _currentElementRecord.Element.OpenEndTag(e.OpenAngleBracketPosition, e.NameToken, _tree.Text.Length);
                } else {
                    var dummyElement = new ElementNode(_tree.RootNode, e.OpenAngleBracketPosition, e.NameToken, _tree.Text.Length);

                    _orphanedEndTagRecord = new ElementRecord(dummyElement);
                    _orphanedEndTagRecord.Element.OpenEndTag(e.OpenAngleBracketPosition, e.NameToken, _tree.Text.Length);
                }
            }
        }

        private void OnEndTagClose(object sender, HtmlParserCloseTagEventArgs e) {
            if (_currentElementRecord != null) {
                ElementNode element = _currentElementRecord.Element;
                ReadOnlyCollection<ElementNode> children = ElementNode.EmptyCollection;
                if (_currentElementRecord.Children.Count > 0) {
                    children = new ReadOnlyCollection<ElementNode>(_currentElementRecord.Children);
                }

                ReadOnlyCollection<AttributeNode> startTagAttributes = AttributeNode.EmptyCollection;
                if (_currentElementRecord.StartTagAttributes.Count > 0) {
                    startTagAttributes = new ReadOnlyCollection<AttributeNode>(_currentElementRecord.StartTagAttributes);
                }

                ReadOnlyCollection<AttributeNode> endTagAttributes = AttributeNode.EmptyCollection;
                if (_currentElementRecord.EndTagAttributes.Count > 0) {
                    endTagAttributes = new ReadOnlyCollection<AttributeNode>(_currentElementRecord.EndTagAttributes);
                }

                element.EndTag.Complete(endTagAttributes, e.CloseAngleBracket, e.IsClosed, false, false);
                element.CompleteElement(e.CloseAngleBracket, true, children, startTagAttributes, endTagAttributes);

                _currentElementRecord = null;
            }

            CloseOrphanedEndTag(e.CloseAngleBracket, e.IsClosed);
        }

        private void OnComment(object sender, HtmlParserCommentEventArgs e) {
            // Add comment to comment collection, not to the tree. Comments
            // require special handling in incremental change handling.
            // This way we can treat them similarly to server artifacts
            // and analyze separately from regular elements.

            // Generally change in comments requires full parse. However, parser does
            // can discover comments in the range it is currently parsing during
            // incremental tree updates. Collection will determine if comment
            // is already in the collection and ignore the add.

            _tree.CommentCollection.Add(e.CommentToken);
        }

        private ElementRecord CloseElements(ITextRange nameRange, int position, string[] containerNames) {
            ElementRecord last = null;
            int count = 0;
            bool foundSameElement = false;
            bool foundContainer = false;
            bool testImplicitClosureWalkingUp = (containerNames.Length > 0);
            bool foundImplicitlyClosedElement = false;
            string name = _tree.Text.GetText(nameRange);

            // Dev12 764293: Match end tags regardless of parsing mode, and let validation ensure the casing.
            // var comparison = Parser.ParsingMode == ParsingMode.Html ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var comparison = StringComparison.OrdinalIgnoreCase;

            // Walk up the stack looking for element name but stop at possible container
            // so we don't close outer <li> by inner <li> in <ol><li><ol><li> and rather
            // stop at the nearest <ol> which is a 'hard' container for <li>.

            foreach (var elementRecord in _elementStack) {
                ITextRange qualifiedNameRange = elementRecord.Element.QualifiedNameRange;
                string qualifiedName = _tree.Text.GetText(qualifiedNameRange);

                if (String.Compare(qualifiedName, name, comparison) == 0) {
                    foundSameElement = true;
                    count++; // Ensure we close the element with the same name
                    break;
                }

                foreach (var containerName in containerNames) {
                    if (String.Compare(qualifiedName, containerName, comparison) == 0) {
                        foundContainer = true;
                        break;
                    }
                }

                if (foundContainer) {
                    break;
                }

                if (testImplicitClosureWalkingUp && !foundImplicitlyClosedElement) {
                    string[] containerNamesCurrentElement;
                    if (!String.IsNullOrEmpty(qualifiedName) && _tree.HtmlClosureProvider.IsImplicitlyClosed(_tree.Text, qualifiedNameRange, out containerNamesCurrentElement)) {
                        foundImplicitlyClosedElement = true;
                    }
                }

                count++;
            }

            if (foundSameElement || (foundImplicitlyClosedElement && foundContainer)) {
                for (int i = 0; i < count; i++) {
                    last = _elementStack.Pop();
                    ElementNode element = last.Element;

                    ReadOnlyCollection<ElementNode> children = ElementNode.EmptyCollection;
                    if (last.Children.Count > 0) {
                        children = new ReadOnlyCollection<ElementNode>(last.Children);
                    }

                    ReadOnlyCollection<AttributeNode> startTagAttributes = AttributeNode.EmptyCollection;
                    if (last.StartTagAttributes.Count > 0) {
                        startTagAttributes = new ReadOnlyCollection<AttributeNode>(last.StartTagAttributes);
                    }

                    ReadOnlyCollection<AttributeNode> endTagAttributes = AttributeNode.EmptyCollection;
                    if (last.EndTagAttributes.Count > 0) {
                        endTagAttributes = new ReadOnlyCollection<AttributeNode>(last.EndTagAttributes);
                    }

                    element.CompleteElement(TextRange.FromBounds(position, position), true, children, startTagAttributes, endTagAttributes);
                }
            }

            return last;
        }

        private void CloseOrphanedEndTag(ITextRange closeAngleBracket, bool isClosed) {
            if (_orphanedEndTagRecord != null) {
                ReadOnlyCollection<AttributeNode> endTagAttributes = AttributeNode.EmptyCollection;
                if (_orphanedEndTagRecord.EndTagAttributes.Count > 0) {
                    endTagAttributes = new ReadOnlyCollection<AttributeNode>(_orphanedEndTagRecord.EndTagAttributes);
                }

                _orphanedEndTagRecord.Element.EndTag.Complete(endTagAttributes, closeAngleBracket, isClosed, false, false);
                ElementRecord currentElementRecord = _elementStack.Peek();
                currentElementRecord.Element.AddOrphanedEndTag(_orphanedEndTagRecord.Element.EndTag);

                _orphanedEndTagRecord = null;
            }
        }
    }
}
