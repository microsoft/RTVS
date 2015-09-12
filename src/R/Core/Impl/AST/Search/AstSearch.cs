using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Statements;

namespace Microsoft.R.Core.AST
{
    public static class AstSearch
    {
        public static IAstNode FindFirstElement(this AstRoot tree, Func<IAstNode, bool> filter)
        {
            var finder = new SingleElementFinder(filter);
            tree.Accept(finder, null);

            return finder.Result;
        }

        public static IReadOnlyCollection<IAstNode> FindElements(this AstRoot tree, Func<IAstNode, bool> filter)
        {
            MultipleElementFinder finder = new MultipleElementFinder(filter);
            tree.Accept(finder, null);

            if (finder.Result.Count == 0)
            {
                return new IAstNode[0];
            }

            return new ReadOnlyCollection<IAstNode>(finder.Result);
        }

        class SingleElementFinder : IAstVisitor
        {
            public IAstNode Result { get; private set; }
            private Func<IAstNode, bool> _match;

            public SingleElementFinder(Func<IAstNode, bool> filter)
            {
                _match = filter;
            }

            public bool Visit(IAstNode element, object parameter)
            {
                if (!_match(element))
                    return true;

                Result = element;
                return false;
            }
        }

        class MultipleElementFinder : IAstVisitor
        {
            public List<IAstNode> Result { get; private set; }
            private Func<IAstNode, bool> _match;

            public MultipleElementFinder(Func<IAstNode, bool> filter)
            {
                _match = filter;
                Result = new List<IAstNode>();
            }

            public bool Visit(IAstNode element, object parameter)
            {
                if (_match(element))
                    Result.Add(element);

                return true;
            }
        }

        /// <summary>
        /// Locates deepest node of a particular type 
        /// </summary>
        public static T GetNodeOfTypeFromPosition<T>(this AstRoot ast, int position, bool includeEnd = false) where T : class
        {
            return GetSpecificNodeFromPosition(ast, position, (IAstNode n) => { return n is T; }, includeEnd) as T;
        }

        /// <summary>
        /// Locates deepest node that matches partucular criteria 
        /// and contains given position in the text buffer
        /// </summary>
        public static IAstNode GetSpecificNodeFromPosition(this AstRoot ast, int position, Func<IAstNode, bool> match, bool includeEnd = false)
        {
            IAstNode deepestNode = null;
            FindSpecificNode(ast, position, match, ref deepestNode, includeEnd);

            return deepestNode;
        }

        private static void FindSpecificNode(IAstNode node, int position, Func<IAstNode, bool> match, ref IAstNode deepestNode, bool includeEnd = false)
        {
            if (!node.Contains(position) && !(includeEnd && node.End == position))
            {
                return; // not this element
            }

            if (match(node))
            {
                deepestNode = node;
            }

            for (int i = 0; i < node.Children.Count && node.Children[i].Start <= position; i++)
            {
                FindSpecificNode(node.Children[i], position, match, ref deepestNode, includeEnd);
            }
        }

        /// <summary>
        /// Retrieves list of packages available to the current file.
        /// Consists of packages in the base library and packages
        /// added via 'library' statements.
        /// </summary>
        public static IEnumerable<string> GetFilePackageNames(this AstRoot ast)
        {
            // TODO: results can be cached until AST actually changes
            AstLibrarySearch search = new AstLibrarySearch();
            ast.Accept(search, null);

            return search.PackageNames;
        }

        private class AstLibrarySearch : IAstVisitor
        {
            public List<string> PackageNames { get; private set; } = new List<string>();

            public bool Visit(IAstNode node, object parameter)
            {
                KeywordIdentifierStatement kis = node as KeywordIdentifierStatement;
                if (kis != null)
                {
                    if (kis.Keyword.Token.IsKeywordText(node.Root.TextProvider, "library") && kis.Identifier != null)
                    {
                        string packageName = node.Root.TextProvider.GetText(kis.Identifier.Token);
                        this.PackageNames.Add(packageName);
                    }
                }

                return true;
            }
        }
    }
}
